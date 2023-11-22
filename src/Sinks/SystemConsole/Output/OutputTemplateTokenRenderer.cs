using System.IO;
using SerilogSlim.Events;

namespace SerilogSlim.Sinks.SystemConsole.Output;

internal abstract class OutputTemplateTokenRenderer
{
    public abstract void Render(LogEvent logEvent, TextWriter output);
}