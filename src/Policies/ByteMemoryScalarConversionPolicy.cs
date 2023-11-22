using System;
using SerilogSlim.Core;
using SerilogSlim.Events;
using System.Diagnostics.CodeAnalysis;

namespace SerilogSlim.Policies
{
    internal class ByteMemoryScalarConversionPolicy : IScalarConversionPolicy
    {
        const int MaximumByteArrayLength = 1024;
        const int MaxTake = 16;

        public bool TryConvertToScalar(object value, [NotNullWhen(true)] out ScalarValue? result)
        {
            if (value is ReadOnlyMemory<byte> x)
            {
                result = new(ConvertToHexString(x));
                return true;
            }

            if (value is Memory<byte> y)
            {
                result = new(ConvertToHexString(y));
                return true;
            }

            result = null;
            return false;
        }

        static string ConvertToHexString(ReadOnlyMemory<byte> bytes)
        {
            if (bytes.Length > MaximumByteArrayLength)
            {
                return ConvertToHexString(bytes[..MaxTake], $"... ({bytes.Length} bytes)");
            }

            return ConvertToHexString(bytes, tail: "");
        }

        static string ConvertToHexString(ReadOnlyMemory<byte> src, string tail)
        {
            var stringLength = src.Length * 2 + tail.Length;

            return string.Create(stringLength, (src, tail), (dest, state) =>
            {
                var (memory, s) = state;

                var byteSpan = memory.Span;
                foreach (var b in byteSpan)
                {
                    if (b.TryFormat(dest, out var written, "X2"))
                    {
                        dest = dest[written..];
                    }
                }

                for (var i = 0; i < s.Length; ++i)
                {
                    dest[i] = s[i];
                }
            });
        }
    }
}
