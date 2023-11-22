using System.Collections.Generic;
using System.IO;
using SerilogSlim.Events;
using SerilogSlim.Parsing;
using SerilogSlim.Rendering;
using SerilogSlim.Sinks.SystemConsole.Themes;
using Microsoft.Extensions.Logging;

namespace SerilogSlim.Sinks.SystemConsole.Output;

internal class LevelTokenRenderer : OutputTemplateTokenRenderer
{
    readonly ConsoleTheme _theme;
    readonly PropertyToken _levelToken;

    static readonly Dictionary<LogLevel, ConsoleThemeStyle> Levels = new Dictionary<LogLevel, ConsoleThemeStyle>
    {
        { LogLevel.Trace, ConsoleThemeStyle.LevelVerbose },
        { LogLevel.Debug, ConsoleThemeStyle.LevelDebug },
        { LogLevel.Information, ConsoleThemeStyle.LevelInformation },
        { LogLevel.Warning, ConsoleThemeStyle.LevelWarning },
        { LogLevel.Error, ConsoleThemeStyle.LevelError },
        { LogLevel.Critical, ConsoleThemeStyle.LevelFatal },
    };

    public LevelTokenRenderer(ConsoleTheme theme, PropertyToken levelToken)
    {
        _theme = theme;
        _levelToken = levelToken;
    }

    public override void Render(LogEvent logEvent, TextWriter output)
    {
        var moniker = LevelOutputFormat.GetLevelMoniker(logEvent.Level, _levelToken.Format);
        if (!Levels.TryGetValue(logEvent.Level, out var levelStyle))
            levelStyle = ConsoleThemeStyle.Invalid;

        var _ = 0;
        using (_theme.Apply(output, levelStyle, ref _))
            Padding.Apply(output, moniker, _levelToken.Alignment);
    }
}