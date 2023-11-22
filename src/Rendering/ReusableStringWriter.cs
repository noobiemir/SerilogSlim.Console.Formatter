using System;
using System.Globalization;
using System.IO;

namespace SerilogSlim.Rendering;

internal class ReusableStringWriter : StringWriter
{
    [ThreadStatic]
    static ReusableStringWriter? _pooledWriter;

    public static StringWriter GetOrCreate(IFormatProvider? formatProvider = null)
    {
        var fmtProvider = formatProvider ?? CultureInfo.CurrentCulture;
        var writer = _pooledWriter;
        _pooledWriter = null;
        if (writer == null || !Equals(writer.FormatProvider, fmtProvider))
        {
            writer = new ReusableStringWriter(formatProvider);
        }

        return writer;
    }

    ReusableStringWriter(IFormatProvider? formatProvider) : base(formatProvider ?? CultureInfo.CurrentCulture)
    {
    }

    protected override void Dispose(bool disposing)
    {
        // We don't call base.Dispose because all it does is mark the writer as closed so it can't be
        // written to and we want to keep it open as reusable writer.
        GetStringBuilder().Clear();
        _pooledWriter = this;
    }
}