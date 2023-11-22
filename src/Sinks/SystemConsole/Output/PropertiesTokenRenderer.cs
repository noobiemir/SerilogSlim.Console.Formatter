using System;
using System.IO;
using System.Linq;
using SerilogSlim.Events;
using SerilogSlim.Parsing;
using SerilogSlim.Rendering;
using SerilogSlim.Sinks.SystemConsole.Formatting;
using SerilogSlim.Sinks.SystemConsole.Rendering;
using SerilogSlim.Sinks.SystemConsole.Themes;
using System.Text;

namespace SerilogSlim.Sinks.SystemConsole.Output
{
    internal class PropertiesTokenRenderer : OutputTemplateTokenRenderer
    {
        readonly MessageTemplate _outputTemplate;
        readonly ConsoleTheme _theme;
        readonly PropertyToken _token;
        readonly ThemedValueFormatter _valueFormatter;

        public PropertiesTokenRenderer(ConsoleTheme theme, PropertyToken token, MessageTemplate outputTemplate, IFormatProvider? formatProvider)
        {
            _outputTemplate = outputTemplate;
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _token = token ?? throw new ArgumentNullException(nameof(token));

            var isJson = false;

            if (token.Format != null)
            {
                foreach (var c in token.Format)
                {
                    if (c == 'j')
                        isJson = true;
                }
            }

            _valueFormatter = isJson
                ? new ThemedJsonValueFormatter(theme, formatProvider)
                : new ThemedDisplayValueFormatter(theme, formatProvider);
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            var included = logEvent.Properties
                .Where(p => !TemplateContainsPropertyName(logEvent.MessageTemplate, p.Key) &&
                            !TemplateContainsPropertyName(_outputTemplate, p.Key))
                .Select(p => new LogEventProperty(p.Key, p.Value));

            var value = new StructureValue(included);

            if (_token.Alignment is null || !_theme.CanBuffer)
            {
                _valueFormatter.Format(value, output, null);
                return;
            }

            var buffer = new StringWriter(new StringBuilder(value.Properties.Count * 16));
            var invisible = _valueFormatter.Format(value, buffer, null);
            var str = buffer.ToString();
            Padding.Apply(output, str, _token.Alignment.Value.Widen(invisible));
        }

        static bool TemplateContainsPropertyName(MessageTemplate template, string propertyName)
        {
            foreach (var token in template.Tokens)
            {
                if (token is PropertyToken namedProperty &&
                    namedProperty.PropertyName == propertyName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
