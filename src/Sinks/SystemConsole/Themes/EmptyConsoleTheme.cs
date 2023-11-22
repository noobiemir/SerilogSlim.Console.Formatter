using System.IO;

namespace SerilogSlim.Sinks.SystemConsole.Themes;

internal class EmptyConsoleTheme : ConsoleTheme
{
    public override bool CanBuffer => true;

    public override int Set(TextWriter output, ConsoleThemeStyle style) => 0;

    public override void Reset(TextWriter output)
    {
    }

    protected override int ResetCharCount => 0;
}