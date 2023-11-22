using System;
using SerilogSlim.Capturing;
using SerilogSlim.Events;
using SerilogSlim.Formatting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace SerilogSlim;

internal class ColorConsoleFormatter : ConsoleFormatter, IDisposable
{
    internal const string OriginalFormatPropertyName = "{OriginalFormat}";
    internal const string FormatterName = "Color";

    internal static readonly ConcurrentDictionary<string, string> DestructureDictionary = new();
    internal static readonly ConcurrentDictionary<string, string> StringifyDictionary = new();

    static readonly CachingMessageTemplateParser MessageTemplateParser = new();

    static readonly LogEventProperty[] LowEventIdValues = Enumerable.Range(0, 48)
        .Select(n => new LogEventProperty("Id", new ScalarValue(n)))
        .ToArray();

    internal static string GetKeyWithoutFirstSymbol(ConcurrentDictionary<string, string> source, string key)
    {
        if (source.TryGetValue(key, out var value))
            return value;
        if (source.Count < 1000)
            return source.GetOrAdd(key, k => k.Substring(1));
        return key.Substring(1);
    }

    readonly PropertyValueConverter _propertyValueConverter;
    private readonly ITextFormatter _textFormatter;
    private readonly IDisposable? _optionsReloadToken;
    private ColorConsoleFormatterOptions _formatterOptions;

    public ColorConsoleFormatter(
        PropertyValueConverter propertyValueConverter,
        ITextFormatter textFormatter,
        IOptionsMonitor<ColorConsoleFormatterOptions> options)
        : base(FormatterName)
    {
        _propertyValueConverter = propertyValueConverter;
        _textFormatter = textFormatter;
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var logEvent = PrepareWrite(logEntry);
        _textFormatter.Format(logEvent, textWriter);
    }

    private LogEvent PrepareWrite<TState>(in LogEntry<TState> logEntry)
    {
        string? messageTemplate = null;
        var properties = new List<LogEventProperty>();

        if (logEntry.State is IEnumerable<KeyValuePair<string, object?>> structure)
        {
            foreach (var property in structure)
            {
                if (property is { Key: OriginalFormatPropertyName, Value: string value })
                {
                    messageTemplate = value;
                }
                else if (property.Key.StartsWith("@"))
                {
                    if (BindProperty(GetKeyWithoutFirstSymbol(DestructureDictionary, property.Key), property.Value, true, out var destructured))
                        properties.Add(destructured);
                }
                else if (property.Key.StartsWith("$"))
                {
                    if (BindProperty(GetKeyWithoutFirstSymbol(StringifyDictionary, property.Key), property.Value?.ToString(), true, out var stringified))
                        properties.Add(stringified);
                }
                else
                {
                    if (BindProperty(property.Key, property.Value, false, out var bound))
                        properties.Add(bound);
                }
            }

            var stateType = logEntry.State.GetType();
            var stateTypeInfo = stateType.GetTypeInfo();
            // Imperfect, but at least eliminates `1 names
            if (messageTemplate == null && !stateTypeInfo.IsGenericType)
            {
                messageTemplate = "{" + stateType.Name + ":l}";
                if (BindProperty(stateType.Name, AsLoggableValue(logEntry.State, logEntry.Formatter), false, out var stateTypeProperty))
                    properties.Add(stateTypeProperty);
            }
        }

        if (messageTemplate == null)
        {
            string propertyName;
            if (logEntry.State != null)
            {
                propertyName = "State";
                messageTemplate = "{State:l}";
            }
            else
            {
                propertyName = "Message";
                messageTemplate = "{Message:l}";
            }

            if (BindProperty(propertyName, AsLoggableValue(logEntry.State, logEntry.Formatter), false, out var property))
                properties.Add(property);
        }

        if (!string.IsNullOrEmpty(logEntry.Category) && BindProperty(nameof(logEntry.Category), logEntry.Category, false, out var categoryProperty))
            properties.Add(categoryProperty);

        if (logEntry.EventId.Id != 0 || logEntry.EventId.Name != null)
            properties.Add(CreateEventIdProperty(logEntry.EventId));

        var parsedTemplate = MessageTemplateParser.Parse(messageTemplate);
        return new LogEvent(GetCurrentDateTime(), logEntry.LogLevel, logEntry.Exception, parsedTemplate, properties);
    }

    private bool BindProperty(string? propertyName, object? value, bool destructureObjects, [NotNullWhen(true)] out LogEventProperty? property)
    {
        if (!LogEventProperty.IsValidName(propertyName))
        {
            property = null;
            return false;
        }

        property = _propertyValueConverter.CreateProperty(propertyName, value, destructureObjects);
        return true;
    }

    static object? AsLoggableValue<TState>(TState state, Func<TState, Exception?, string>? formatter)
    {
        object? stateObj = state;
        if (formatter != null)
            stateObj = formatter(state, null);
        return stateObj;
    }

    internal static LogEventProperty CreateEventIdProperty(EventId eventId)
    {
        var properties = new List<LogEventProperty>(2);

        if (eventId.Id != 0)
        {
            if (eventId.Id >= 0 && eventId.Id < LowEventIdValues.Length)
                // Avoid some allocations
                properties.Add(LowEventIdValues[eventId.Id]);
            else
                properties.Add(new LogEventProperty("Id", new ScalarValue(eventId.Id)));
        }

        if (eventId.Name != null)
        {
            properties.Add(new LogEventProperty("Name", new ScalarValue(eventId.Name)));
        }

        return new LogEventProperty("EventId", new StructureValue(properties));
    }

    [MemberNotNull(nameof(_formatterOptions))]
    private void ReloadLoggerOptions(ColorConsoleFormatterOptions options)
    {
        _formatterOptions = options;
    }

    private DateTimeOffset GetCurrentDateTime()
    {
        return _formatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }
}