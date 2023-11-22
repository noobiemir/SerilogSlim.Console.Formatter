using System;
using System.Collections.Generic;
using System.IO;
using SerilogSlim.Events;
using SerilogSlim.Formatting;
using SerilogSlim.Formatting.Display;
using SerilogSlim.Parsing;
using SerilogSlim.Sinks.SystemConsole.Themes;

namespace SerilogSlim.Sinks.SystemConsole.Output;

internal class OutputTemplateRenderer : ITextFormatter
{
    private readonly OutputTemplateTokenRenderer[] _renders;

    public OutputTemplateRenderer(ConsoleTheme theme, string outputTemplate, IFormatProvider? formatProvider)
    {
        if (outputTemplate is null) throw new ArgumentNullException(nameof(outputTemplate));
        var template = new MessageTemplateParser().Parse(outputTemplate);

        var renders = new List<OutputTemplateTokenRenderer>();

        foreach (var token in template.Tokens)
        {
            if (token is TextToken tt)
            {
                renders.Add(new TextTokenRenderer(theme, tt.Text));
                continue;
            }

            var pt = (PropertyToken)token;
            if (pt.PropertyName == OutputProperties.LevelPropertyName)
            {
                renders.Add(new LevelTokenRenderer(theme, pt));
            }
            else if (pt.PropertyName == OutputProperties.NewLinePropertyName)
            {
                renders.Add(new NewLineTokenRenderer(pt.Alignment));
            }
            else if (pt.PropertyName == OutputProperties.ExceptionPropertyName)
            {
                renders.Add(new ExceptionTokenRenderer(theme));
            }
            else if (pt.PropertyName == OutputProperties.MessagePropertyName)
            {
                renders.Add(new MessageTemplateOutputTokenRenderer(theme, pt, formatProvider));
            }
            else if (pt.PropertyName == OutputProperties.TimestampPropertyName)
            {
                renders.Add(new TimestampTokenRenderer(theme, pt, formatProvider));
            }
            else if (pt.PropertyName == OutputProperties.PropertiesPropertyName)
            {
                renders.Add(new PropertiesTokenRenderer(theme, pt, template, formatProvider));
            }
            else
            {
                renders.Add(new EventPropertyTokenRenderer(theme, pt, formatProvider));
            }
        }

        _renders = renders.ToArray();
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        if (logEvent is null) throw new ArgumentNullException(nameof(logEvent));
        if (output is null) throw new ArgumentNullException(nameof(output));

        foreach (var renderer in _renders)
            renderer.Render(logEvent, output);
    }
}