namespace SerilogSlim.Sinks.SystemConsole.Themes;

static class AnsiEscapeSequence
{
    public const string Unthemed = "";
    public const string Reset = "\x1b[0m";
    public const string Bold = "\x1b[1m";

    public const string Black = "\x1b[30m";
    public const string Red = "\x1b[31m";
    public const string Green = "\x1b[32m";
    public const string Yellow = "\x1b[33m";
    public const string Blue = "\x1b[34m";
    public const string Magenta = "\x1b[35m";
    public const string Cyan = "\x1b[36m";
    public const string White = "\x1b[37m";

    public const string BrightBlack = "\x1b[30;1m";
    public const string BrightRed = "\x1b[31;1m";
    public const string BrightGreen = "\x1b[32;1m";
    public const string BrightYellow = "\x1b[33;1m";
    public const string BrightBlue = "\x1b[34;1m";
    public const string BrightMagenta = "\x1b[35;1m";
    public const string BrightCyan = "\x1b[36;1m";
    public const string BrightWhite = "\x1b[37;1m";
}