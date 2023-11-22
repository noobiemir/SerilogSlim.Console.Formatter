﻿using System;
using System.IO;
using SerilogSlim.Events;
using SerilogSlim.Formatting.Json;
using SerilogSlim.Sinks.SystemConsole.Themes;

namespace SerilogSlim.Sinks.SystemConsole.Formatting
{
    internal class ThemedDisplayValueFormatter : ThemedValueFormatter
    {
        readonly IFormatProvider? _formatProvider;

        public ThemedDisplayValueFormatter(ConsoleTheme theme, IFormatProvider? formatProvider)
            : base(theme)
        {
            _formatProvider = formatProvider;
        }

        protected override int VisitScalarValue(ThemedValueFormatterState state, ScalarValue scalar)
        {
            if (scalar is null)
                throw new ArgumentNullException(nameof(scalar));
            return FormatLiteralValue(scalar, state.Output, state.Format);
        }

        protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
        {
            if (sequence is null)
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
                Visit(state, value);
            }

            using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                state.Output.Write(']');

            return count;
        }

        protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
        {
            var count = 0;

            if (structure.TypeTag != null)
            {
                using (ApplyStyle(state.Output, ConsoleThemeStyle.Name, ref count))
                    state.Output.Write(structure.TypeTag);

                state.Output.Write(' ');
            }

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
                    state.Output.Write(property.Name);

                using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                    state.Output.Write('=');

                count += Visit(state.Nest(), property.Value);
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

                using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                    state.Output.Write('[');

                using (ApplyStyle(state.Output, ConsoleThemeStyle.String, ref count))
                    count += Visit(state.Nest(), element.Key);

                using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                    state.Output.Write("]=");

                count += Visit(state.Nest(), element.Value);
            }

            using (ApplyStyle(state.Output, ConsoleThemeStyle.TertiaryText, ref count))
                state.Output.Write('}');

            return count;
        }

        public override ThemedValueFormatter SwitchTheme(ConsoleTheme theme)
        {
            return new ThemedDisplayValueFormatter(theme, _formatProvider);
        }

        public int FormatLiteralValue(ScalarValue scalar, TextWriter output, string? format)
        {
            var value = scalar.Value;
            var count = 0;

            if (value is null)
            {
                using (ApplyStyle(output, ConsoleThemeStyle.Null, ref count))
                    output.Write("null");
                return count;
            }

            if (value is string str)
            {
                using (ApplyStyle(output, ConsoleThemeStyle.String, ref count))
                {
                    if (format != "l")
                        JsonValueFormatter.WriteQuotedJsonString(str, output);
                    else
                        output.Write(str);
                }
                return count;
            }

            if (value is ValueType)
            {
                if (value is int || value is uint || value is long || value is ulong ||
                    value is decimal || value is byte || value is sbyte || value is short ||
                    value is ushort || value is float || value is double)
                {
                    using (ApplyStyle(output, ConsoleThemeStyle.Number, ref count))
                        scalar.Render(output, format, _formatProvider);
                    return count;
                }

                if (value is bool b)
                {
                    using (ApplyStyle(output, ConsoleThemeStyle.Boolean, ref count))
                        output.Write(b);

                    return count;
                }

                if (value is char ch)
                {
                    using (ApplyStyle(output, ConsoleThemeStyle.Scalar, ref count))
                    {
                        output.Write('\'');
                        output.Write(ch);
                        output.Write('\'');
                    }
                    return count;
                }
            }

            using (ApplyStyle(output, ConsoleThemeStyle.Scalar, ref count))
                scalar.Render(output, format, _formatProvider);

            return count;
        }
    }
}
