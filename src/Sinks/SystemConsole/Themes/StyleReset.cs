using System;
using System.IO;

namespace SerilogSlim.Sinks.SystemConsole.Themes;

internal readonly struct StyleReset : IDisposable
{
    private readonly ConsoleTheme _theme;
    private readonly TextWriter _output;

    public StyleReset(ConsoleTheme theme, TextWriter output)
    {
        _theme = theme;
        _output = output;
    }
    
    public void Dispose()
    {
        _theme.Reset(_output);
    }
}