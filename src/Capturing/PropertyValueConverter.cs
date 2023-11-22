using System;
using SerilogSlim.Core;
using SerilogSlim.Debugging;
using SerilogSlim.Events;
using SerilogSlim.Parsing;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SerilogSlim.Policies;

namespace SerilogSlim.Capturing
{
    internal partial class PropertyValueConverter : ILogEventPropertyFactory, ILogEventPropertyValueFactory
    {
        static readonly HashSet<Type> BuiltInScalarTypes = new()
        {
            typeof(bool),
            typeof(char),
            typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
            typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal),
            typeof(string),
            typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
            typeof(Guid), typeof(Uri),
            typeof(TimeOnly), typeof(DateOnly)
        };

        private readonly IDestructuringPolicy[] _destructuringPolicies;
        private readonly Type[] _dictionaryTypes;
        private readonly IScalarConversionPolicy[] _scalarConversionPolicies;
        private readonly DepthLimiter _depthLimiter;
        private readonly int _maximumStringLength;
        private readonly int _maximumCollectionCount;
        private readonly bool _propagateExceptions;

        public PropertyValueConverter(
            int maximumDestructuringDepth,
            int maximumStringLength,
            int maximumCollectionCount,
            IEnumerable<Type> additionalScalarTypes,
            IEnumerable<Type> additionalDictionaryTypes,
            IEnumerable<IDestructuringPolicy> additionalDestructuringPolicies,
            bool propagateExceptions)
        {
            if (additionalScalarTypes == null) throw new ArgumentNullException(nameof(additionalScalarTypes));
            if (additionalDestructuringPolicies == null) throw new ArgumentNullException(nameof(additionalDestructuringPolicies));
            if (maximumDestructuringDepth < 0) throw new ArgumentOutOfRangeException(nameof(maximumDestructuringDepth));
            if (maximumStringLength < 2) throw new ArgumentOutOfRangeException(nameof(maximumStringLength));
            if (maximumCollectionCount < 1) throw new ArgumentOutOfRangeException(nameof(maximumCollectionCount));

            _propagateExceptions = propagateExceptions;
            _maximumStringLength = maximumStringLength;
            _maximumCollectionCount = maximumCollectionCount;

            _scalarConversionPolicies = new IScalarConversionPolicy[]
            {
                new SimpleScalarConversionPolicy(BuiltInScalarTypes.Concat(additionalScalarTypes)),
                new EnumScalarConversionPolicy(),
                new ByteArrayScalarConversionPolicy(),
                new ByteMemoryScalarConversionPolicy(),
            };

            _destructuringPolicies = additionalDestructuringPolicies
                .Concat(new IDestructuringPolicy[]
                {
                    new DelegateDestructuringPolicy(),
                    new ReflectionTypesScalarDestructuringPolicy()
                })
                .ToArray();

            _dictionaryTypes = additionalDictionaryTypes.ToArray();
            _depthLimiter = new(maximumDestructuringDepth, this);
        }

        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
        {
            return new(name, CreatePropertyValue(value, destructureObjects));
        }

        public LogEventPropertyValue CreatePropertyValue(object? value, bool destructureObjects = false)
        {
            return CreatePropertyValue(value, destructureObjects, 1);
        }

        LogEventPropertyValue CreatePropertyValue(object? value, bool destructureObjects, int depth)
        {
            return CreatePropertyValue(
                value,
                destructureObjects ?
                    Destructuring.Destructure :
                    Destructuring.Default,
                depth);
        }

        LogEventPropertyValue CreatePropertyValue(object? value, Destructuring destructuring, int depth)
        {
            if (value == null)
                return ScalarValue.Null;

            if (destructuring == Destructuring.Stringify)
            {
                return Stringify(value);
            }

            if (destructuring == Destructuring.Destructure)
            {
                if (value is string stringValue)
                {
                    value = TruncateIfNecessary(stringValue);
                }
            }

            if (value is string)
                return new ScalarValue(value);

            foreach (var scalarConversionPolicy in _scalarConversionPolicies)
            {
                if (scalarConversionPolicy.TryConvertToScalar(value, out var converted))
                    return converted;
            }

            DepthLimiter.SetCurrentDepth(depth);

            if (destructuring == Destructuring.Destructure)
            {
                foreach (var destructuringPolicy in _destructuringPolicies)
                {
                    if (destructuringPolicy.TryDestructure(value, _depthLimiter, out var result))
                        return result;
                }
            }

            var type = value.GetType();
            if (TryConvertEnumerable(value, type, destructuring, out var enumerableResult))
                return enumerableResult;

            if (TryConvertValueTuple(value, destructuring, out var tupleResult))
                return tupleResult;

            if (TryConvertCompilerGeneratedType(value, type, destructuring, out var compilerGeneratedResult))
                return compilerGeneratedResult;

            return new ScalarValue(value.ToString() ?? "");
        }

        bool TryConvertEnumerable(object value, Type type, Destructuring destructuring, [NotNullWhen(true)] out LogEventPropertyValue? result)
        {
            if (value is IEnumerable enumerable)
            {
                // Only dictionaries with 'scalar' keys are permitted, as
                // more complex keys may not serialize to unique values for
                // representation in sinks. This check strengthens the expectation
                // that resulting dictionary is representable in JSON as well
                // as richer formats (e.g. XML, .NET type-aware...).
                // Only actual dictionaries are supported, as arbitrary types
                // can implement multiple IDictionary interfaces and thus introduce
                // multiple different interpretations.
                if (TryGetDictionary(value, type, out var dictionary))
                {
                    result = new DictionaryValue(MapToDictionaryElements(dictionary, destructuring));
                    return true;

                    IEnumerable<KeyValuePair<ScalarValue, LogEventPropertyValue>> MapToDictionaryElements(IDictionary dictionaryEntries, Destructuring destructure)
                    {
                        var count = 0;
                        foreach (DictionaryEntry entry in dictionaryEntries)
                        {
                            if (++count > _maximumCollectionCount)
                            {
                                yield break;
                            }

                            var pair = new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                                (ScalarValue)_depthLimiter.CreatePropertyValue(entry.Key, destructure),
                                _depthLimiter.CreatePropertyValue(entry.Value, destructure));

                            if (pair.Key.Value != null)
                                yield return pair;
                        }
                    }
                }

                // Avoids allocation of two iterators - one from List and another one from MapToSequenceElements.
                // Allocation free for empty sequence.
                if (enumerable is IList list && list.Count <= _maximumCollectionCount)
                {
                    if (list.Count == 0)
                    {
                        result = SequenceValue.Empty;
                    }
                    else
                    {
                        var array = new LogEventPropertyValue[list.Count];
                        for (int i = 0; i < list.Count; ++i)
                            array[i] = _depthLimiter.CreatePropertyValue(list[i], destructuring);
                        result = new SequenceValue(array);
                    }
                }
                else
                {
                    result = new SequenceValue(MapToSequenceElements(enumerable, destructuring));
                }
                return true;

                IEnumerable<LogEventPropertyValue> MapToSequenceElements(IEnumerable sequence, Destructuring destructure)
                {
                    var count = 0;
                    foreach (var element in sequence)
                    {
                        if (++count > _maximumCollectionCount)
                        {
                            yield break;
                        }

                        yield return _depthLimiter.CreatePropertyValue(element, destructure);
                    }
                }
            }

            result = null;
            return false;
        }

        private bool TryConvertValueTuple(object value, Destructuring destructuring, [NotNullWhen(true)] out LogEventPropertyValue? result)
        {
            if (value is not ITuple tuple)
            {
                result = null;
                return false;
            }

            var elements = new LogEventPropertyValue[tuple.Length];
            for (var i = 0; i < tuple.Length; i++)
            {
                var fieldValue = tuple[i];
                elements[i] = _depthLimiter.CreatePropertyValue(fieldValue, destructuring);
            }

            result = new SequenceValue(elements);
            return true;
        }

        bool TryConvertCompilerGeneratedType(
            object value,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type,
            Destructuring destructuring,
            [NotNullWhen(true)] out LogEventPropertyValue? result)
        {
            if (destructuring == Destructuring.Destructure)
            {
                var typeTag = type.Name;
                if (typeTag.Length <= 0 || IsCompilerGeneratedType(type))
                {
                    typeTag = null;
                }

                result = new StructureValue(GetProperties(value, type), typeTag);
                return true;
            }

            result = null;
            return false;
        }

        LogEventPropertyValue Stringify(object value)
        {
            var stringField = value.ToString();
            var truncated = stringField == null ? "" : TruncateIfNecessary(stringField);
            return new ScalarValue(truncated);
        }

        string TruncateIfNecessary(string text)
        {
            if (text.Length > _maximumStringLength)
            {
                return text.Substring(0, _maximumStringLength - 1) + "…";
            }

            return text;
        }

        bool TryGetDictionary(object value, Type valueType, [NotNullWhen(true)] out IDictionary? dictionary)
        {
            if (value is IDictionary iDictionary)
            {
                if (_dictionaryTypes.Contains(valueType))
                {
                    dictionary = iDictionary;
                    return true;
                }

                if (valueType.IsConstructedGenericType)
                {
                    var definition = valueType.GetGenericTypeDefinition();
                    if ((definition == typeof(Dictionary<,>) || definition == typeof(System.Collections.ObjectModel.ReadOnlyDictionary<,>)) &&
                        IsValidDictionaryKeyType(valueType.GenericTypeArguments[0]))
                    {
                        dictionary = iDictionary;
                        return true;
                    }
                }
            }

            dictionary = null;
            return false;
        }

        static bool IsValidDictionaryKeyType(Type valueType)
        {
            return BuiltInScalarTypes.Contains(valueType) ||
                   valueType.IsEnum;
        }

        IEnumerable<LogEventProperty> GetProperties(object value, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
        {
            foreach (var prop in type.GetPropertiesRecursive())
            {
                object? propValue;
                try
                {
                    propValue = prop.GetValue(value);
                }
                catch (TargetParameterCountException)
                {
                    // These properties would ideally be ignored; since they never produce values they're not
                    // of concern to auditing and exceptions can be suppressed.
                    SelfLog.WriteLine("The property accessor {0} is a non-default indexer", prop);
                    continue;
                }
                catch (TargetInvocationException ex)
                {
                    SelfLog.WriteLine("The property accessor {0} threw exception: {1}", prop, ex);

                    if (_propagateExceptions)
                        throw;

                    propValue = "The property accessor threw an exception: " + ex.InnerException?.GetType().Name;
                }
                catch (NotSupportedException)
                {
                    SelfLog.WriteLine("The property accessor {0} is not supported via Reflection API", prop);

                    if (_propagateExceptions)
                        throw;

                    propValue = "Accessing this property is not supported via Reflection API";
                }
                yield return new(prop.Name, _depthLimiter.CreatePropertyValue(propValue, Destructuring.Destructure));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCompilerGeneratedType(Type type)
        {
            if (!type.IsGenericType || !type.IsSealed || type.Namespace != null)
            {
                return false;
            }

            // C# Anonymous types always start with "<>" and VB's start with "VB$"
            var name = type.Name;
            return name[0] == '<'
                   || (name.Length > 2 && name[0] == 'V' && name[1] == 'B' && name[2] == '$');
        }
    }
}
