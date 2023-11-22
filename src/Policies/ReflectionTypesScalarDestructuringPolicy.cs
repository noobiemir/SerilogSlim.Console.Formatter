using System;
using SerilogSlim.Core;
using SerilogSlim.Events;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SerilogSlim.Policies;

internal class ReflectionTypesScalarDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        // These types and their subclasses are property-laden and deep;
        // most sinks will convert them to strings.
        if (value is Type or MemberInfo)
        {
            result = new ScalarValue(value);
            return true;
        }

        result = null;
        return false;
    }
}