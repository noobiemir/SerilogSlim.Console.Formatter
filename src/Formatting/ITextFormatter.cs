using System.IO;
using SerilogSlim.Events;

namespace SerilogSlim.Formatting;

internal interface ITextFormatter
{
    void Format(LogEvent logEvent, TextWriter output);
}