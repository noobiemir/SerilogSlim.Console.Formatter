using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerilogSlim.Parsing;
using SerilogSlim.Debugging;
using SerilogSlim.Rendering;

namespace SerilogSlim.Events
{
    internal class MessageTemplate
    {
        public MessageTemplate(string text, IEnumerable<MessageTemplateToken> tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            TokenArray = tokens.ToArray();

            var propertyTokens = GetElementsOfTypeToArray<PropertyToken>(TokenArray);
            if (propertyTokens.Length != 0)
            {
                var allPositional = true;
                var anyPositional = false;
                foreach (var propertyToken in propertyTokens)
                {
                    if (propertyToken.IsPositional)
                        anyPositional = true;
                    else
                        allPositional = false;
                }

                if (allPositional)
                {
                    PositionalProperties = propertyTokens;
                }
                else
                {
                    if (anyPositional)
                        SelfLog.WriteLine("Message template is malformed: {0}", text);

                    NamedProperties = propertyTokens;
                }
            }
        }

        public string Text { get; }

        public IEnumerable<MessageTemplateToken> Tokens => TokenArray;

        internal MessageTemplateToken[] TokenArray { get; }

        internal PropertyToken[]? NamedProperties { get; }

        internal PropertyToken[]? PositionalProperties { get; }

        static TResult[] GetElementsOfTypeToArray<TResult>(MessageTemplateToken[] tokens)
            where TResult : class
        {
            var result = new List<TResult>(tokens.Length / 2);
            foreach (var mtt in tokens)
            {
                if (mtt is TResult token)
                {
                    result.Add(token);
                }
            }
            return result.ToArray();
        }

        public string Render(IReadOnlyDictionary<string, LogEventPropertyValue> properties, IFormatProvider? formatProvider = null)
        {
            using var writer = ReusableStringWriter.GetOrCreate(formatProvider);
            Render(properties, writer, formatProvider);
            return writer.ToString();
        }

        public void Render(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider? formatProvider = null)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (output == null) throw new ArgumentNullException(nameof(output));
            MessageTemplateRenderer.Render(this, properties, output, null, formatProvider);
        }
    }
}