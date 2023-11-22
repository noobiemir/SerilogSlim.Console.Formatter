using System;
using System.Collections.Generic;
using System.IO;
using SerilogSlim.Events;
using SerilogSlim.Rendering;

namespace SerilogSlim.Parsing;

internal sealed class TextToken : MessageTemplateToken
{
    public TextToken(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public override int Length => Text.Length;

    public override void Render(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider? formatProvider = null)
    {
        if (output == null) throw new ArgumentNullException(nameof(output));
        MessageTemplateRenderer.RenderTextToken(this, output);
    }
    public override bool Equals(object? obj)
    {
        return obj is TextToken tt && tt.Text == Text;
    }

    public override int GetHashCode() => Text.GetHashCode();

    public override string ToString() => Text;

    public string Text { get; }
}