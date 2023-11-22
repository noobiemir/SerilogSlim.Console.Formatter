using Microsoft.Extensions.Logging.Console;

namespace SerilogSlim;

internal class ColorConsoleFormatterOptions : ConsoleFormatterOptions
{
    internal const string DefaultConsoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
}