using System;
using System.IO;
using SerilogSlim.Events;
using SerilogSlim.Parsing;
using SerilogSlim.Rendering;
using SerilogSlim.Sinks.SystemConsole.Themes;

namespace SerilogSlim.Sinks.SystemConsole.Output
{
    internal class EventPropertyTokenRenderer : OutputTemplateTokenRenderer
    {
        readonly ConsoleTheme _theme;
        readonly PropertyToken _token;
        readonly IFormatProvider? _formatProvider;

        public EventPropertyTokenRenderer(ConsoleTheme theme, PropertyToken token, IFormatProvider? formatProvider)
        {
            _theme = theme;
            _token = token;
            _formatProvider = formatProvider;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            // If a property is missing, don't render anything (message templates render the raw token here).
            if (!logEvent.Properties.TryGetValue(_token.PropertyName, out var propertyValue))
            {
                Padding.Apply(output, string.Empty, _token.Alignment);
                return;
            }

            var _ = 0;
            using (_theme.Apply(output, ConsoleThemeStyle.SecondaryText, ref _))
            {
                var writer = _token.Alignment.HasValue ? new StringWriter() : output;

                // If the value is a scalar string, support some additional formats: 'u' for uppercase
                // and 'w' for lowercase.
                if (propertyValue is ScalarValue sv && sv.Value is string literalString)
                {
                    var cased = Format(literalString, _token.Format);
                    writer.Write(cased);
                }
                else
                {
                    propertyValue.Render(writer, _token.Format, _formatProvider);
                }

                if (!_token.Alignment.HasValue) return;
                var str = writer.ToString()!;
                Padding.Apply(output, str, _token.Alignment);
            }
        }

        public static string Format(string value, string? format = null)
        {
            return format switch
            {
                "u" => value.ToUpperInvariant(),
                "w" => value.ToLowerInvariant(),
                _ => value
            };
        }
    }
}
