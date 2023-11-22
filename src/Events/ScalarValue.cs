using System.Globalization;
using System;
using System.IO;

namespace SerilogSlim.Events;

internal class ScalarValue : LogEventPropertyValue
{
    public static ScalarValue Null { get; } = new(null);

    public ScalarValue(object? value)
    {
        Value = value;
    }

    public object? Value { get; }

    public override void Render(TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        Render(Value, output, format, formatProvider);
    }

    internal static void Render(object? value, TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        if (output == null) throw new ArgumentNullException(nameof(output));

        if (value == null)
        {
            output.Write("null");
            return;
        }

        if (value is string s)
        {
            if (format != "l")
            {
                output.Write('"');
                output.Write(s.Replace("\"", "\\\""));
                output.Write('"');
            }
            else
            {
                output.Write(s);
            }
            return;
        }

        var custom = (ICustomFormatter?)formatProvider?.GetFormat(typeof(ICustomFormatter));
        if (custom != null)
        {
            output.Write(custom.Format(format, value, formatProvider));
            return;
        }

        if (value is IFormattable f)
        {
            output.Write(f.ToString(format, formatProvider ?? CultureInfo.InvariantCulture));
        }
        else
        {
            output.Write(value.ToString());
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is ScalarValue sv && Equals(Value, sv.Value);
    }

    public override int GetHashCode()
    {
        if (Value == null) return 0;
        return Value.GetHashCode();
    }
}