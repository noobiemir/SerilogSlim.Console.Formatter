using System;
using System.Collections.Generic;
using SerilogSlim.Core;
using SerilogSlim.Events;
using System.Diagnostics.CodeAnalysis;

namespace SerilogSlim.Policies;

internal class SimpleScalarConversionPolicy : IScalarConversionPolicy
{
    readonly HashSet<Type> _scalarTypes;

    public SimpleScalarConversionPolicy(IEnumerable<Type> scalarTypes)
    {
        _scalarTypes = new(scalarTypes);
    }

    public bool TryConvertToScalar(object value, [NotNullWhen(true)] out ScalarValue? result)
    {
        if (_scalarTypes.Contains(value.GetType()))
        {
            result = new(value);
            return true;
        }

        result = null;
        return false;
    }
}