using System;
using SerilogSlim.Events;

namespace SerilogSlim.Data;

internal abstract class LogEventPropertyValueVisitor<TState, TResult>
{
    protected virtual TResult Visit(TState state, LogEventPropertyValue value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (value is ScalarValue sv)
            return VisitScalarValue(state, sv);

        if (value is SequenceValue seqv)
            return VisitSequenceValue(state, seqv);

        if (value is StructureValue strv)
            return VisitStructureValue(state, strv);

        if (value is DictionaryValue dictv)
            return VisitDictionaryValue(state, dictv);

        return VisitUnsupportedValue(state, value);
    }

    protected abstract TResult VisitScalarValue(TState state, ScalarValue scalar);

    protected abstract TResult VisitSequenceValue(TState state, SequenceValue sequence);

    protected abstract TResult VisitStructureValue(TState state, StructureValue structure);

    protected abstract TResult VisitDictionaryValue(TState state, DictionaryValue dictionary);

    protected virtual TResult VisitUnsupportedValue(TState state, LogEventPropertyValue value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        throw new NotSupportedException($"The value {value} is not of a type supported by this visitor.");
    }
}