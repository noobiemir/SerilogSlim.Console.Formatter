using System;
using System.IO;
using SerilogSlim.Events;
using SerilogSlim.Parsing;
using SerilogSlim.Rendering;
using SerilogSlim.Sinks.SystemConsole.Formatting;
using SerilogSlim.Sinks.SystemConsole.Rendering;
using SerilogSlim.Sinks.SystemConsole.Themes;

namespace SerilogSlim.Sinks.SystemConsole.Output;

internal class MessageTemplateOutputTokenRenderer : OutputTemplateTokenRenderer
{
    readonly ConsoleTheme _theme;
    readonly PropertyToken _token;
    readonly ThemedMessageTemplateRenderer _renderer;

    public MessageTemplateOutputTokenRenderer(ConsoleTheme theme, PropertyToken token, IFormatProvider? formatProvider)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _token = token ?? throw new ArgumentNullException(nameof(token));

        bool isLiteral = false, isJson = false;

        if (token.Format != null)
        {
            foreach (var c in token.Format)
            {
                if (c == 'l')
                    isLiteral = true;
                else if (c == 'j')
                    isJson = true;
            }
        }

        var valueFormatter = isJson
            ? (ThemedValueFormatter)new ThemedJsonValueFormatter(theme, formatProvider)
            : new ThemedDisplayValueFormatter(theme, formatProvider);

        _renderer = new ThemedMessageTemplateRenderer(theme, valueFormatter, isLiteral);
    }

    public override void Render(LogEvent logEvent, TextWriter output)
    {
        if (_token.Alignment is null || !_theme.CanBuffer)
        {
            _renderer.Render(logEvent.MessageTemplate, logEvent.Properties, output);
            return;
        }

        var buffer = new StringWriter();
        var invisible = _renderer.Render(logEvent.MessageTemplate, logEvent.Properties, buffer);
        var value = buffer.ToString();
        Padding.Apply(output, value, _token.Alignment.Value.Widen(invisible));
    }
}