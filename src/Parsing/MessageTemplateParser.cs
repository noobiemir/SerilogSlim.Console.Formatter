using System;
using System.Collections.Generic;
using SerilogSlim.Events;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SerilogSlim.Parsing;

internal class MessageTemplateParser : IMessageTemplateParser
{
    public MessageTemplate Parse(string messageTemplate)
    {
        if (messageTemplate == null) throw new ArgumentNullException(nameof(messageTemplate));
        return new(messageTemplate, Tokenize(messageTemplate));
    }

    private static readonly TextToken EmptyTextToken = new("");

    private static IEnumerable<MessageTemplateToken> Tokenize(string messageTemplate)
    {
        if (messageTemplate.Length == 0)
        {
            yield return EmptyTextToken;
            yield break;
        }

        var nextIndex = 0;
        while (true)
        {
            var beforeText = nextIndex;
            var tt = ParseTextToken(nextIndex, messageTemplate, out nextIndex);
            if (nextIndex > beforeText)
                yield return tt;

            if (nextIndex == messageTemplate.Length)
                yield break;

            var beforeProp = nextIndex;
            var pt = ParsePropertyToken(nextIndex, messageTemplate, out nextIndex);
            if (beforeProp < nextIndex)
                yield return pt;

            if (nextIndex == messageTemplate.Length)
                yield break;
        }
    }

    private static MessageTemplateToken ParsePropertyToken(int startAt, string messageTemplate, out int next)
    {
        var first = startAt;
        startAt++;
        while (startAt < messageTemplate.Length && IsValidInPropertyTag(messageTemplate[startAt]))
            startAt++;

        if (startAt == messageTemplate.Length || messageTemplate[startAt] != '}')
        {
            next = startAt;
            return new TextToken(messageTemplate[first..next]);
        }

        next = startAt + 1;

        var rawText = messageTemplate[first..next];
        var tagContent = rawText.Substring(1, next - (first + 2));
        if (tagContent.Length == 0)
            return new TextToken(rawText);

        if (!TrySplitTagContent(tagContent, out var propertyNameAndDestructuring, out var format, out var alignment))
            return new TextToken(rawText);

        var propertyName = propertyNameAndDestructuring;
        var destructuring = Destructuring.Default;
        if (propertyName.Length != 0 && TryGetDestructuringHint(propertyName[0], out destructuring))
            propertyName = propertyName[1..];

        if (propertyName.Length == 0)
            return new TextToken(rawText);

        if (propertyName.Any(c => !IsValidInPropertyName(c)))
        {
            return new TextToken(rawText);
        }

        if (format != null)
        {
            if (format.Any(c => !IsValidInFormat(c)))
            {
                return new TextToken(rawText);
            }
        }

        Alignment? alignmentValue = null;
        if (alignment != null)
        {
            if (alignment.Any(c => !IsValidInAlignment(c)))
            {
                return new TextToken(rawText);
            }

            var lastDash = alignment.LastIndexOf('-');
            if (lastDash > 0)
                return new TextToken(rawText);

            if (!int.TryParse(lastDash == -1 ? alignment : alignment.Substring(1), out var width) || width == 0)
                return new TextToken(rawText);

            var direction = lastDash == -1 ?
                AlignmentDirection.Right :
                AlignmentDirection.Left;

            alignmentValue = new(direction, width);
        }

        return new PropertyToken(
            propertyName,
            rawText,
            format,
            alignmentValue,
            destructuring);
    }

    private static bool TrySplitTagContent(string tagContent, [NotNullWhen(true)] out string? propertyNameAndDestructuring, out string? format, out string? alignment)
    {
        var formatDelim = tagContent.IndexOf(':');
        var alignmentDelim = tagContent.IndexOf(',');
        if (formatDelim == -1 && alignmentDelim == -1)
        {
            propertyNameAndDestructuring = tagContent;
            format = null;
            alignment = null;
            return true;
        }

        if (alignmentDelim == -1 || (formatDelim != -1 && alignmentDelim > formatDelim))
        {
            propertyNameAndDestructuring = tagContent.Substring(0, formatDelim);
            format = formatDelim == tagContent.Length - 1 ?
                null :
                tagContent.Substring(formatDelim + 1);
            alignment = null;
            return true;
        }

        propertyNameAndDestructuring = tagContent.Substring(0, alignmentDelim);
        if (formatDelim == -1)
        {
            if (alignmentDelim == tagContent.Length - 1)
            {
                alignment = format = null;
                return false;
            }

            format = null;
            alignment = tagContent.Substring(alignmentDelim + 1);
            return true;
        }

        if (alignmentDelim == formatDelim - 1)
        {
            alignment = format = null;
            return false;
        }

        alignment = tagContent.Substring(alignmentDelim + 1, formatDelim - alignmentDelim - 1);
        format = formatDelim == tagContent.Length - 1 ?
            null :
            tagContent.Substring(formatDelim + 1);

        return true;
    }

    private static bool IsValidInPropertyTag(char c)
    {
        return IsValidInDestructuringHint(c) ||
               IsValidInPropertyName(c) ||
               IsValidInFormat(c) ||
               c == ':';
    }

    private static bool IsValidInPropertyName(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static bool TryGetDestructuringHint(char c, out Destructuring destructuring)
    {
        switch (c)
        {
            case '@':
                destructuring = Destructuring.Destructure;
                return true;
            case '$':
                destructuring = Destructuring.Stringify;
                return true;
            default:
                destructuring = Destructuring.Default;
                return false;
        }
    }

    private static bool IsValidInDestructuringHint(char c)
    {
        return c is '@' or '$';
    }

    private static bool IsValidInAlignment(char c)
    {
        return char.IsDigit(c) ||
               c == '-';
    }

    private static bool IsValidInFormat(char c)
    {
        return c != '}' &&
               (char.IsLetterOrDigit(c) ||
                char.IsPunctuation(c) ||
                c is ' ' or '+');
    }

    private static TextToken ParseTextToken(int startAt, string messageTemplate, out int next)
    {
        var accum = messageTemplate.Length < 5 * 1024 ? new ValueStringBuilder(stackalloc char[messageTemplate.Length]) : new ValueStringBuilder(messageTemplate.Length);

        do
        {
            var nc = messageTemplate[startAt];
            if (nc == '{')
            {
                if (startAt + 1 < messageTemplate.Length &&
                    messageTemplate[startAt + 1] == '{')
                {
                    accum.Append(nc);
                    startAt++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                accum.Append(nc);
                if (nc == '}')
                {
                    if (startAt + 1 < messageTemplate.Length &&
                        messageTemplate[startAt + 1] == '}')
                    {
                        startAt++;
                    }
                }
            }

            startAt++;
        } while (startAt < messageTemplate.Length);

        next = startAt;
        return new(accum.ToString());
    }
}