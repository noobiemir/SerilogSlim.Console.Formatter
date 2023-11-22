using System;
using SerilogSlim.Events;
using SerilogSlim.Formatting.Json;
using SerilogSlim.Sinks.SystemConsole.Themes;
using System.Globalization;
using System.IO;

namespace SerilogSlim.Sinks.SystemConsole.Formatting
{
    internal class ThemedJsonValueFormatter : ThemedValueFormatter
    {
        readonly ThemedDisplayValueFormatter _displayFormatter;
        readonly IFormatProvider? _formatProvider;

        public ThemedJsonValueFormatter(ConsoleTheme theme, IFormatProvider? formatProvider)
            : base(theme)
        {
            _displayFormatter = new ThemedDisplayValueFormatter(theme, formatProvider);
            _formatProvider = formatProvider;
        }

        public override ThemedValueFormatter SwitchTheme(ConsoleTheme theme)
        {
            return new ThemedJsonValueFormatter(theme, _formatProvider);
        }

        protected override int VisitScalarValue(ThemedValueFormatterState state, ScalarValue scalar)
        {
            if (scalar is null)
                throw new ArgumentNullException(nameof(scalar));

            // At the top level, for scalar values, use "display" rendering.
            if (state.IsTopLevel)
                return _displayFormatter.FormatLiteralValue(scalar, state.Output, state.Format);

            return FormatLiteralValue(scalar, state.Output);
        }

        protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var count = 0;

            using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                state.Output.Write('[');

            var delim = string.Empty;
            foreach (var value in sequence.Elements)
            {
                if (delim.Length != 0)
                {
                    using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                        state.Output.Write(delim);
                }

                delim = ", ";
                Visit(state.Nest(), value);
            }

            using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                state.Output.Write(']');

            return count;
        }

        protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
        {
            var count = 0;

            using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                state.Output.Write('{');

            var delim = string.Empty;
            foreach (var property in structure.Properties)
            {
                if (delim.Length != 0)
                {
                    using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                        state.Output.Write(delim);
                }

                delim = ", ";

                using (ApplyStyle(state.Output, ConsoleThemeStyle.Name, ref count))
                    JsonValueFormatter.WriteQuotedJsonString(property.Name, state.Output);

                using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                    state.Output.Write(": ");

                count += Visit(state.Nest(), property.Value);
            }

            if (structure.TypeTag != null)
            {
                using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                    state.Output.Write(delim);

                using (ApplyStyle(state.Output, ConsoleThemeStyle.Name, ref count))
                    JsonValueFormatter.WriteQuotedJsonString("$type", state.Output);

                using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                    state.Output.Write(": ");

                using (ApplyStyle(state.Output, ConsoleThemeStyle.String, ref count))
                    JsonValueFormatter.WriteQuotedJsonString(structure.TypeTag, state.Output);
            }

            using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                state.Output.Write('}');

            return count;
        }

        protected override int VisitDictionaryValue(ThemedValueFormatterState state, DictionaryValue dictionary)
        {
            var count = 0;

            using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                state.Output.Write('{');

            var delim = string.Empty;
            foreach (var element in dictionary.Elements)
            {
                if (delim.Length != 0)
                {
                    using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                        state.Output.Write(delim);
                }

                delim = ", ";

                var style = element.Key.Value == null
                    ? ConsoleThemeStyle.Null
                    : element.Key.Value is string
                        ? ConsoleThemeStyle.String
                        : ConsoleThemeStyle.Scalar;

                using (ApplyStyle(state.Output, style, ref count))
                    JsonValueFormatter.WriteQuotedJsonString((element.Key.Value ?? "null").ToString() ?? "", state.Output);

                using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                    state.Output.Write(": ");

                count += Visit(state.Nest(), element.Value);
            }

            using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                state.Output.Write('}');

            return count;
        }

        int FormatLiteralValue(ScalarValue scalar, TextWriter output)
        {
            var value = scalar.Value;
            var count = 0;

            if (value == null)
            {
                using (ApplyStyle(output, ConsoleThemeStyle.Null, ref count))
                    output.Write("null");
                return count;
            }

            if (value is string str)
            {
                using (ApplyStyle(output, ConsoleThemeStyle.String, ref count))
                    JsonValueFormatter.WriteQuotedJsonString(str, output);
                return count;
            }

            if (value is ValueType)
            {
                if (value is int || value is uint || value is long || value is ulong || value is decimal || value is byte || value is sbyte || value is short || value is ushort)
                {
                    using (ApplyStyle(output, ConsoleThemeStyle.Number, ref count))
                        output.Write(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture));
                    return count;
                }

                if (value is double d)
                {
                    using (ApplyStyle(output, ConsoleThemeStyle.Number, ref count))
                    {
                        if (double.IsNaN(d) || double.IsInfinity(d))
                            JsonValueFormatter.WriteQuotedJsonString(d.ToString(CultureInfo.InvariantCulture), output);
                        else
                            output.Write(d.ToString("R", CultureInfo.InvariantCulture));
                    }
                    return count;
                }

                if (value is float f)
                {
                    using (ApplyStyle(output, ConsoleThemeStyle.Number, ref count))
                    {
                        if (double.IsNaN(f) || double.IsInfinity(f))
                            JsonValueFormatter.WriteQuotedJsonString(f.ToString(CultureInfo.InvariantCulture), output);
                        else
                            output.Write(f.ToString("R", CultureInfo.InvariantCulture));
                    }
                    return count;
                }

                if (value is bool b)
                {
                    using (ApplyStyle(output, ConsoleThemeStyle.Boolean, ref count))
                        output.Write(b ? "true" : "false");

                    return count;
                }

                if (value is char ch)
                {
                    using (ApplyStyle(output, ConsoleThemeStyle.Scalar, ref count))
                        JsonValueFormatter.WriteQuotedJsonString(ch.ToString(), output);
                    return count;
                }

                if (value is DateTime || value is DateTimeOffset)
                {
                    using (ApplyStyle(output, ConsoleThemeStyle.Scalar, ref count))
                    {
                        output.Write('"');
                        output.Write(((IFormattable)value).ToString("O", CultureInfo.InvariantCulture));
                        output.Write('"');
                    }
                    return count;
                }
            }

            using (ApplyStyle(output, ConsoleThemeStyle.Scalar, ref count))
                JsonValueFormatter.WriteQuotedJsonString(value.ToString() ?? "", output);

            return count;
        }
    }
}
