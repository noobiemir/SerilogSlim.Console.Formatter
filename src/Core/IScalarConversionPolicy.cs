using SerilogSlim.Events;
using System.Diagnostics.CodeAnalysis;

namespace SerilogSlim.Core
{
    internal interface IScalarConversionPolicy
    {
        bool TryConvertToScalar(object value, [NotNullWhen(true)] out ScalarValue? result);
    }
}
