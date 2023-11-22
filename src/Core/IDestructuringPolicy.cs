using System.Diagnostics.CodeAnalysis;
using SerilogSlim.Events;

namespace SerilogSlim.Core;

internal interface IDestructuringPolicy
{
    bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result);
}