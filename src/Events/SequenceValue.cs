using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SerilogSlim.Events;

internal class SequenceValue : LogEventPropertyValue
{
    readonly LogEventPropertyValue[] _elements;

    /// <summary>
    /// Empty sequence of values.
    /// </summary>
    public static SequenceValue Empty { get; } = new(Array.Empty<LogEventPropertyValue>());

    public SequenceValue(IEnumerable<LogEventPropertyValue> elements)
    {
        if (elements == null) throw new ArgumentNullException(nameof(elements));
        _elements = elements.ToArray();
    }

    internal SequenceValue(LogEventPropertyValue[] elements)
    {
        if (elements == null) throw new ArgumentNullException(nameof(elements));
        _elements = elements;
    }

    public IReadOnlyList<LogEventPropertyValue> Elements => _elements;

    public override void Render(TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        if (output == null) throw new ArgumentNullException(nameof(output));

        output.Write('[');
        var allButLast = _elements.Length - 1;
        for (var i = 0; i < allButLast; ++i)
        {
            _elements[i].Render(output, format, formatProvider);
            output.Write(", ");
        }

        if (_elements.Length > 0)
            _elements[^1].Render(output, format, formatProvider);

        output.Write(']');
    }
}