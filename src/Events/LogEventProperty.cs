using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SerilogSlim.Events;

internal class LogEventProperty
{
    public LogEventProperty(string name, LogEventPropertyValue value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        EnsureValidName(name);

        Name = name;
        Value = value;
    }

    internal LogEventProperty(EventProperty property)
    {
        if (property.Equals(EventProperty.None)) throw new ArgumentNullException(nameof(property));

        Name = property.Name;
        Value = property.Value;
    }

    public string Name { get; }

    public LogEventPropertyValue Value { get; }

    public static bool IsValidName([NotNullWhen(true)] string? name) => !string.IsNullOrWhiteSpace(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EnsureValidName(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (!IsValidName(name)) throw new ArgumentException($"Property {nameof(name)} must not be empty or whitespace.", nameof(name));
    }
}