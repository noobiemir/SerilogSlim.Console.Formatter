using System;
using SerilogSlim.Core;
using SerilogSlim.Events;
using System.Diagnostics.CodeAnalysis;

namespace SerilogSlim.Policies;

internal class ByteArrayScalarConversionPolicy : IScalarConversionPolicy
{
    const int MaximumByteArrayLength = 1024;

    public bool TryConvertToScalar(object value, [NotNullWhen(true)] out ScalarValue? result)
    {
        if (value is not byte[] bytes)
        {
            result = null;
            return false;
        }

        if (bytes.Length > MaximumByteArrayLength)
        {
            var start = Convert.ToHexString(bytes, 0, 16);
            var description = start + "... (" + bytes.Length + " bytes)";
            result = new ScalarValue(description);
        }
        else
        {
            result = new ScalarValue(Convert.ToHexString(bytes));
        }

        return true;
    }
}