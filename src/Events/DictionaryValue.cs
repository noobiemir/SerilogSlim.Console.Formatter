using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SerilogSlim.Events;

internal class DictionaryValue : LogEventPropertyValue
{
    public DictionaryValue(IEnumerable<KeyValuePair<ScalarValue, LogEventPropertyValue>> elements)
    {
        if (elements == null) throw new ArgumentNullException(nameof(elements));

        Elements = elements.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public IReadOnlyDictionary<ScalarValue, LogEventPropertyValue> Elements { get; }

    public override void Render(TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        if (output == null) throw new ArgumentNullException(nameof(output));
        output.Write('[');
        var delim = "(";
        foreach (var kvp in Elements)
        {
            output.Write(delim);
            delim = ", (";
            kvp.Key.Render(output, null, formatProvider);
            output.Write(": ");
            kvp.Value.Render(output, null, formatProvider);
            output.Write(')');
        }

        output.Write(']');
    }
}