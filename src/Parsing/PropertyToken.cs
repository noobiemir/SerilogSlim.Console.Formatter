using SerilogSlim.Events;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using SerilogSlim.Rendering;

namespace SerilogSlim.Parsing;

internal class PropertyToken : MessageTemplateToken
{
    readonly int? _position;

    public PropertyToken(string propertyName, string rawText, string? format = null, in Alignment? alignment = null, Destructuring destructuring = Destructuring.Default)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Format = format;
        Destructuring = destructuring;
        RawText = rawText ?? throw new ArgumentNullException(nameof(rawText));
        Alignment = alignment;

        if (int.TryParse(PropertyName, NumberStyles.None, CultureInfo.InvariantCulture, out var position) &&
            position >= 0)
        {
            _position = position;
        }
    }

    public override int Length => RawText.Length;

    public string PropertyName { get; }

    public Destructuring Destructuring { get; }

    public string? Format { get; }

    public Alignment? Alignment { get; }

    /// <summary>
    /// <see langword="true"/> if the property name is a positional index; otherwise, <see langword="false"/>.
    /// </summary>
    public bool IsPositional => _position.HasValue;

    internal string RawText { get; }

    public bool TryGetPositionalValue(out int position)
    {
        if (_position == null)
        {
            position = 0;
            return false;
        }

        position = _position.Value;
        return true;
    }

    public override void Render(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider? formatProvider = null)
    {
        MessageTemplateRenderer.RenderPropertyToken(this, properties, output, formatProvider, isLiteral: false, isJson: false);
    }

    public override string ToString() => RawText;

    public override bool Equals(object? obj)
    {
        return obj is PropertyToken pt &&
               pt.Destructuring == Destructuring &&
               pt.Format == Format &&
               pt.PropertyName == PropertyName &&
               pt.RawText == RawText;
    }

    public override int GetHashCode() => PropertyName.GetHashCode();
}