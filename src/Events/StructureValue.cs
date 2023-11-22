using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SerilogSlim.Events;

internal class StructureValue : LogEventPropertyValue
{
    readonly LogEventProperty[] _properties;

    public StructureValue(IEnumerable<LogEventProperty> properties, string? typeTag = null)
    {
        if (properties == null) throw new ArgumentNullException(nameof(properties));
        TypeTag = typeTag;
        _properties = properties.ToArray();
    }

    public string? TypeTag { get; }

    public IReadOnlyList<LogEventProperty> Properties => _properties;

    public override void Render(TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        if (output == null) throw new ArgumentNullException(nameof(output));

        if (TypeTag != null)
        {
            output.Write(TypeTag);
            output.Write(' ');
        }
        output.Write("{ ");
        var allButLast = _properties.Length - 1;
        for (var i = 0; i < allButLast; i++)
        {
            var property = _properties[i];
            Render(output, property, formatProvider);
            output.Write(", ");
        }

        if (_properties.Length > 0)
        {
            var last = _properties[^1];
            Render(output, last, formatProvider);
        }

        output.Write(" }");
    }

    static void Render(TextWriter output, LogEventProperty property, IFormatProvider? formatProvider = null)
    {
        output.Write(property.Name);
        output.Write(": ");
        property.Value.Render(output, null, formatProvider);
    }
}