using System;
using System.Collections.Generic;
using System.IO;
using SerilogSlim.Events;
using SerilogSlim.Formatting.Json;
using SerilogSlim.Parsing;
using System.Runtime.CompilerServices;

namespace SerilogSlim.Rendering;

internal static class MessageTemplateRenderer
{
    static readonly JsonValueFormatter JsonValueFormatter = new("$type");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Render(MessageTemplate messageTemplate, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        bool isLiteral = false, isJson = false;

        if (format != null)
        {
            foreach (var c in format)
            {
                if (c == 'l')
                    isLiteral = true;
                else if (c == 'j')
                    isJson = true;
            }
        }

        foreach (var token in messageTemplate.TokenArray)
        {
            if (token is TextToken tt)
            {
                RenderTextToken(tt, output);
            }
            else
            {
                var pt = (PropertyToken)token;
                RenderPropertyToken(pt, properties, output, formatProvider, isLiteral, isJson);
            }
        }
    }

    public static void RenderTextToken(TextToken tt, TextWriter output)
    {
        output.Write(tt.Text);
    }

    public static void RenderPropertyToken(PropertyToken pt, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider? formatProvider, bool isLiteral, bool isJson)
    {
        if (!properties.TryGetValue(pt.PropertyName, out var propertyValue))
        {
            output.Write(pt.RawText);
            return;
        }

        if (!pt.Alignment.HasValue)
        {
            RenderValue(propertyValue, isLiteral, isJson, output, pt.Format, formatProvider);
            return;
        }

        using var valueOutput = ReusableStringWriter.GetOrCreate();
        RenderValue(propertyValue, isLiteral, isJson, valueOutput, pt.Format, formatProvider);
        var sb = valueOutput.GetStringBuilder();

        if (sb.Length >= pt.Alignment.Value.Width)
        {
            output.Write(sb);
            return;
        }

        Padding.Apply(output, sb, pt.Alignment.Value);
    }

    static void RenderValue(LogEventPropertyValue propertyValue, bool literal, bool json, TextWriter output, string? format, IFormatProvider? formatProvider)
    {
        if (literal && propertyValue is ScalarValue { Value: string str })
        {
            output.Write(str);
        }
        else if (json && format == null)
        {
            JsonValueFormatter.Format(propertyValue, output);
        }
        else
        {
            propertyValue.Render(output, format, formatProvider);
        }
    }
}