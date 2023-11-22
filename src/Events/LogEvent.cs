using System.Reflection.Emit;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using SerilogSlim.Core;

namespace SerilogSlim.Events;

internal class LogEvent
{
    readonly Dictionary<string, LogEventPropertyValue> _properties;

    LogEvent(DateTimeOffset timestamp, LogLevel level, Exception? exception, MessageTemplate messageTemplate, Dictionary<string, LogEventPropertyValue> properties)
    {
        Timestamp = timestamp;
        Level = level;
        Exception = exception;
        MessageTemplate = messageTemplate ?? throw new ArgumentNullException(nameof(messageTemplate));
        _properties = properties ?? throw new ArgumentNullException(nameof(properties));
    }

    public LogEvent(DateTimeOffset timestamp, LogLevel level, Exception? exception, MessageTemplate messageTemplate, IEnumerable<LogEventProperty> properties)
        : this(timestamp, level, exception, messageTemplate, new Dictionary<string, LogEventPropertyValue>())
    {
        if (properties == null) throw new ArgumentNullException(nameof(properties));
        foreach (var property in properties)
            AddOrUpdateProperty(property);
    }

    internal LogEvent(DateTimeOffset timestamp, LogLevel level, Exception? exception, MessageTemplate messageTemplate, EventProperty[] properties)
        : this(timestamp, level, exception, messageTemplate, new Dictionary<string, LogEventPropertyValue>(AgainstNull(properties).Length))
    {
        foreach (var eventProperty in properties)
            _properties[eventProperty.Name] = eventProperty.Value;
    }

    static T AgainstNull<T>([NotNull] T? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return argument;
    }

    public DateTimeOffset Timestamp { get; }

    public LogLevel Level { get; }

    public MessageTemplate MessageTemplate { get; }

    public IReadOnlyDictionary<string, LogEventPropertyValue> Properties => _properties;

    public Exception? Exception { get; }

    public void RenderMessage(TextWriter output, IFormatProvider? formatProvider = null)
    {
        MessageTemplate.Render(Properties, output, formatProvider);
    }

    public string RenderMessage(IFormatProvider? formatProvider = null)
    {
        return MessageTemplate.Render(Properties, formatProvider);
    }

    public void AddOrUpdateProperty(LogEventProperty property)
    {
        if (property == null) throw new ArgumentNullException(nameof(property));
        _properties[property.Name] = property.Value;
    }

    internal void AddOrUpdateProperty(in EventProperty property)
    {
        if (property.Equals(EventProperty.None)) throw new ArgumentNullException(nameof(property));

        _properties[property.Name] = property.Value;
    }

    public void AddPropertyIfAbsent(LogEventProperty property)
    {
        if (property == null) throw new ArgumentNullException(nameof(property));
        _properties.TryAdd(property.Name, property.Value);
    }

    internal void AddPropertyIfAbsent(in EventProperty property)
    {
        if (property.Equals(EventProperty.None)) throw new ArgumentNullException(nameof(property));
        _properties.TryAdd(property.Name, property.Value);
    }

    internal void AddPropertyIfAbsent(ILogEventPropertyFactory factory, string name, object? value, bool destructureObjects = false)
    {
        if (!_properties.ContainsKey(name))
        {
            _properties.Add(
                name,
                factory is ILogEventPropertyValueFactory factory2
                    ? factory2.CreatePropertyValue(value, destructureObjects)
                    : factory.CreateProperty(name, value, destructureObjects).Value);
        }
    }

    public void RemovePropertyIfPresent(string propertyName)
    {
        _properties.Remove(propertyName);
    }

    internal LogEvent Copy()
    {
        var properties = new Dictionary<string, LogEventPropertyValue>(_properties);

        return new LogEvent(
            Timestamp,
            Level,
            Exception,
            MessageTemplate,
            properties);
    }
}