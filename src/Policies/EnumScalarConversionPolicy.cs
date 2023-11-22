using System;
using SerilogSlim.Core;
using SerilogSlim.Events;
using System.Diagnostics.CodeAnalysis;

namespace SerilogSlim.Policies
{
    internal class EnumScalarConversionPolicy : IScalarConversionPolicy
    {
        public bool TryConvertToScalar(object value, [NotNullWhen(true)] out ScalarValue? result)
        {
            if (value is Enum)
            {
                result = new ScalarValue(value);
                return true;
            }

            result = null;
            return false;
        }
    }
}
