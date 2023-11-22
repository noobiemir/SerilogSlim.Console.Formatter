using SerilogSlim.Events;

namespace SerilogSlim.Core;

internal interface ILogEventPropertyValueFactory
{
    LogEventPropertyValue CreatePropertyValue(object? value, bool destructureObjects = false);
}