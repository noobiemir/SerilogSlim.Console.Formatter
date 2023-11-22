using System.IO;
using SerilogSlim.Events;
using SerilogSlim.Sinks.SystemConsole.Themes;

namespace SerilogSlim.Sinks.SystemConsole.Output;

internal class TextTokenRenderer : OutputTemplateTokenRenderer
{
    private readonly ConsoleTheme _theme;
    private readonly string _text;

    public TextTokenRenderer(ConsoleTheme theme, string text)
    {
        _theme = theme;
        _text = text;
    }

    public override void Render(LogEvent logEvent, TextWriter output)
    {
        var _ = 0;
        using (_theme.Apply(output, ConsoleThemeStyle.TertiaryText, ref _))
            output.Write(_text);
    }
}