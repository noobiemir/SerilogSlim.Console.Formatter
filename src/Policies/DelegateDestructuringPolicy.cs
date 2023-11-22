using System;
using SerilogSlim.Core;
using SerilogSlim.Events;
using System.Diagnostics.CodeAnalysis;

namespace SerilogSlim.Policies;

internal class DelegateDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        if (value is Delegate del)
        {
            result = new ScalarValue(del.ToString());
            return true;
        }

        result = null;
        return false;
    }
}