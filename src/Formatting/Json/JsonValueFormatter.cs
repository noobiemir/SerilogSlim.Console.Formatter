using System;
using SerilogSlim.Data;
using SerilogSlim.Events;
using System.Globalization;
using System.IO;

namespace SerilogSlim.Formatting.Json;

internal class JsonValueFormatter : LogEventPropertyValueVisitor<TextWriter, bool>
{
    readonly string? _typeTagName;

    const string DefaultTypeTagName = "_typeTag";

    public void Format(LogEventPropertyValue value, TextWriter output)
    {
        Visit(output, value);
    }

    public JsonValueFormatter(string? typeTagName = DefaultTypeTagName)
    {
        _typeTagName = typeTagName;
    }

    protected override bool VisitScalarValue(TextWriter state, ScalarValue scalar)
    {
        if (scalar == null) throw new ArgumentNullException(nameof(scalar));
        FormatLiteralValue(scalar.Value, state);
        return false;
    }

    protected override bool VisitSequenceValue(TextWriter state, SequenceValue sequence)
    {
        if (sequence == null) throw new ArgumentNullException(nameof(sequence));
        state.Write('[');
        char? delim = null;
        foreach (var t in sequence.Elements)
        {
            if (delim != null)
            {
                state.Write(delim.Value);
            }
            delim = ',';
            Visit(state, t);
        }
        state.Write(']');
        return false;
    }

    protected override bool VisitStructureValue(TextWriter state, StructureValue structure)
    {
        state.Write('{');

        char? delim = null;

        foreach (var t in structure.Properties)
        {
            if (delim != null)
            {
                state.Write(delim.Value);
            }
            delim = ',';
            var prop = t;
            WriteQuotedJsonString(prop.Name, state);
            state.Write(':');
            Visit(state, prop.Value);
        }

        if (_typeTagName != null && structure.TypeTag != null)
        {
            state.Write(delim);
            WriteQuotedJsonString(_typeTagName, state);
            state.Write(':');
            WriteQuotedJsonString(structure.TypeTag, state);
        }

        state.Write('}');
        return false;
    }

    protected override bool VisitDictionaryValue(TextWriter state, DictionaryValue dictionary)
    {
        state.Write('{');
        char? delim = null;
        foreach (var element in dictionary.Elements)
        {
            if (delim != null)
            {
                state.Write(delim.Value);
            }
            delim = ',';
            WriteQuotedJsonString((element.Key.Value ?? "null").ToString()!, state);
            state.Write(':');
            Visit(state, element.Value);
        }
        state.Write('}');
        return false;
    }

    protected virtual void FormatLiteralValue(object? value, TextWriter output)
    {
        if (value == null)
        {
            FormatNullValue(output);
            return;
        }

        // Although the linear switch-on-type has apparently worse algorithmic performance than the O(1)
        // dictionary lookup alternative, in practice, it's much to make a few equality comparisons
        // than the hash/bucket dictionary lookup, and since most data will be string (one comparison),
        // numeric (a handful) or an object (two comparisons) the real-world performance of the code
        // as written is as fast or faster.

        if (value is string str)
        {
            FormatStringValue(str, output);
            return;
        }

        if (value is ValueType)
        {
            if (value is int i)
            {
                FormatExactNumericValue(i, output);
                return;
            }

            if (value is uint ui)
            {
                FormatExactNumericValue(ui, output);
                return;
            }

            if (value is long l)
            {
                FormatExactNumericValue(l, output);
                return;
            }

            if (value is ulong ul)
            {
                FormatExactNumericValue(ul, output);
                return;
            }

            if (value is decimal dc)
            {
                FormatExactNumericValue(dc, output);
                return;
            }

            if (value is byte bt)
            {
                FormatExactNumericValue(bt, output);
                return;
            }

            if (value is sbyte sb)
            {
                FormatExactNumericValue(sb, output);
                return;
            }

            if (value is short s)
            {
                FormatExactNumericValue(s, output);
                return;
            }

            if (value is ushort us)
            {
                FormatExactNumericValue(us, output);
                return;
            }

            if (value is double d)
            {
                FormatDoubleValue(d, output);
                return;
            }

            if (value is float f)
            {
                FormatFloatValue(f, output);
                return;
            }

            if (value is bool b)
            {
                FormatBooleanValue(b, output);
                return;
            }

            if (value is char)
            {
                FormatStringValue(value.ToString()!, output);
                return;
            }

            if (value is DateTime dt)
            {
                FormatDateTimeValue(dt, output);
                return;
            }

            if (value is DateTimeOffset dto)
            {
                FormatDateTimeOffsetValue(dto, output);
                return;
            }

            if (value is TimeSpan timeSpan)
            {
                FormatTimeSpanValue(timeSpan, output);
                return;
            }

            if (value is DateOnly dateOnly)
            {
                FormatDateOnlyValue(dateOnly, output);
                return;
            }

            if (value is TimeOnly timeOnly)
            {
                FormatTimeOnlyValue(timeOnly, output);
                return;
            }
        }

        FormatLiteralObjectValue(value, output);
    }

    static void FormatNullValue(TextWriter output)
    {
        output.Write("null");
    }

    static void FormatStringValue(string str, TextWriter output)
    {
        WriteQuotedJsonString(str, output);
    }
    static void FormatExactNumericValue(int value, TextWriter output)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, provider: CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString(null, CultureInfo.InvariantCulture));
    }

    static void FormatExactNumericValue(uint value, TextWriter output)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, provider: CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString(null, CultureInfo.InvariantCulture));
    }

    static void FormatExactNumericValue(long value, TextWriter output)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, provider: CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString(null, CultureInfo.InvariantCulture));
    }

    static void FormatExactNumericValue(ulong value, TextWriter output)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, provider: CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString(null, CultureInfo.InvariantCulture));
    }

    static void FormatExactNumericValue(decimal value, TextWriter output)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, provider: CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString(null, CultureInfo.InvariantCulture));
    }

    static void FormatExactNumericValue(byte value, TextWriter output)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, provider: CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString(null, CultureInfo.InvariantCulture));
    }

    static void FormatExactNumericValue(sbyte value, TextWriter output)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, provider: CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString(null, CultureInfo.InvariantCulture));
    }

    static void FormatExactNumericValue(short value, TextWriter output)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, provider: CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString(null, CultureInfo.InvariantCulture));
    }

    static void FormatExactNumericValue(ushort value, TextWriter output)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, provider: CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString(null, CultureInfo.InvariantCulture));
    }

    static void FormatDoubleValue(double value, TextWriter output)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            FormatStringValue(value.ToString(CultureInfo.InvariantCulture), output);
            return;
        }

        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, "R", CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString("R", CultureInfo.InvariantCulture));
    }
    static void FormatFloatValue(float value, TextWriter output)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            FormatStringValue(value.ToString(CultureInfo.InvariantCulture), output);
            return;
        }

        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, "R", CultureInfo.InvariantCulture))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString("R", CultureInfo.InvariantCulture));
    }

    static void FormatBooleanValue(bool value, TextWriter output)
    {
        output.Write(value ? "true" : "false");
    }

    static void FormatDateTimeValue(DateTime value, TextWriter output)
    {
        output.Write('\"');

        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, format: "O"))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString("O", CultureInfo.InvariantCulture));

        output.Write('\"');
    }

    static void FormatDateTimeOffsetValue(DateTimeOffset value, TextWriter output)
    {
        output.Write('\"');

        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written, format: "O"))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString("O", CultureInfo.InvariantCulture));

        output.Write('\"');
    }

    static void FormatTimeSpanValue(TimeSpan value, TextWriter output)
    {
        output.Write('\"');
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out var written))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString());
        output.Write('\"');
    }

    static void FormatDateOnlyValue(DateOnly value, TextWriter output)
    {
        output.Write('\"');

        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out int written, format: "yyyy-MM-dd"))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString("yyyy-MM-dd"));

        output.Write('\"');
    }

    static void FormatTimeOnlyValue(TimeOnly value, TextWriter output)
    {
        output.Write('\"');

        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out int written, format: "O"))
            output.Write(buffer.Slice(0, written));
        else
            output.Write(value.ToString("O"));

        output.Write('\"');
    }
    static void FormatLiteralObjectValue(object value, TextWriter output)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        FormatStringValue(value.ToString() ?? "", output);
    }

    public static void WriteQuotedJsonString(string str, TextWriter output)
    {
        output.Write('\"');

        var cleanSegmentStart = 0;
        var anyEscaped = false;

        for (var i = 0; i < str.Length; ++i)
        {
            var c = str[i];
            if (c is < (char)32 or '\\' or '"')
            {
                anyEscaped = true;
                output.Write(str.AsSpan().Slice(cleanSegmentStart, i - cleanSegmentStart));
                cleanSegmentStart = i + 1;

                switch (c)
                {
                    case '"':
                        output.Write("\\\"");
                        break;
                    case '\\':
                        output.Write("\\\\");
                        break;
                    case '\n':
                        output.Write("\\n");
                        break;
                    case '\r':
                        output.Write("\\r");
                        break;
                    case '\f':
                        output.Write("\\f");
                        break;
                    case '\t':
                        output.Write("\\t");
                        break;
                    default:
                        output.Write("\\u");
                        output.Write(((int)c).ToString("X4"));
                        break;
                }
            }
        }

        if (anyEscaped)
        {
            if (cleanSegmentStart != str.Length)
                output.Write(str.AsSpan().Slice(cleanSegmentStart));
        }
        else
        {
            output.Write(str);
        }

        output.Write('\"');
    }
}