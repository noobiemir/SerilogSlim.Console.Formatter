using System.IO;
using SerilogSlim.Events;

namespace SerilogSlim.Core;

internal interface ILogEventSink
{
    void Emit(LogEvent logEvent, TextWriter output);
}