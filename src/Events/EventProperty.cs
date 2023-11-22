using System;

namespace SerilogSlim.Events;

readonly struct EventProperty
{
    public static EventProperty None = default;

    public string Name { get; }

    public LogEventPropertyValue Value { get; }

    public EventProperty(string name, LogEventPropertyValue value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        LogEventProperty.EnsureValidName(name);

        Name = name;
        Value = value;
    }

    public void Deconstruct(out string name, out LogEventPropertyValue value)
    {
        name = Name;
        value = Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is EventProperty other && Equals(other);
    }

    public bool Equals(EventProperty other)
    {
        return string.Equals(Name, other.Name) && Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
        }
    }
}