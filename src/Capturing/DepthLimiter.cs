using System;
using SerilogSlim.Core;
using SerilogSlim.Events;
using SerilogSlim.Debugging;
using SerilogSlim.Parsing;

namespace SerilogSlim.Capturing
{
    partial class PropertyValueConverter
    {
        private class DepthLimiter : ILogEventPropertyValueFactory
        {
            [ThreadStatic]
            static int _currentDepth;

            readonly int _maximumDestructuringDepth;
            readonly PropertyValueConverter _propertyValueConverter;

            public DepthLimiter(int maximumDepth, PropertyValueConverter propertyValueConverter)
            {
                _maximumDestructuringDepth = maximumDepth;
                _propertyValueConverter = propertyValueConverter;
            }

            public static void SetCurrentDepth(int depth)
            {
                _currentDepth = depth;
            }

            public LogEventPropertyValue CreatePropertyValue(object? value, Destructuring destructuring)
            {
                var storedDepth = _currentDepth;

                var result = DefaultIfMaximumDepth(storedDepth) ??
                             _propertyValueConverter.CreatePropertyValue(value, destructuring, storedDepth + 1);

                _currentDepth = storedDepth;

                return result;
            }

            LogEventPropertyValue ILogEventPropertyValueFactory.CreatePropertyValue(object? value, bool destructureObjects)
            {
                var storedDepth = _currentDepth;

                var result = DefaultIfMaximumDepth(storedDepth) ??
                             _propertyValueConverter.CreatePropertyValue(value, destructureObjects, storedDepth + 1);

                _currentDepth = storedDepth;

                return result;
            }

            LogEventPropertyValue? DefaultIfMaximumDepth(int depth)
            {
                if (depth == _maximumDestructuringDepth)
                {
                    SelfLog.WriteLine("Maximum destructuring depth reached.");
                    return ScalarValue.Null;
                }

                return null;
            }
        }
    }
}
