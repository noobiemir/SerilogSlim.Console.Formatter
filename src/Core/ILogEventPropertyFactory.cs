using SerilogSlim.Events;

namespace SerilogSlim.Core;

internal interface ILogEventPropertyFactory
{
    LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false);
}