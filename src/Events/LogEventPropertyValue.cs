using System;
using System.IO;
using SerilogSlim.Rendering;

namespace SerilogSlim.Events;

internal abstract class LogEventPropertyValue : IFormattable
{
    public abstract void Render(TextWriter output, string? format = null, IFormatProvider? formatProvider = null);

    public override string ToString() => ToString(null, null);

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        using var output = ReusableStringWriter.GetOrCreate();
        Render(output, format, formatProvider);
        return output.ToString();
    }
}