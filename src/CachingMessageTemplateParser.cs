using System;
using SerilogSlim.Events;
using SerilogSlim.Parsing;
using System.Collections;

namespace SerilogSlim
{
    internal class CachingMessageTemplateParser
    {
        readonly MessageTemplateParser _innerParser = new();

        readonly object _templatesLock = new();
        readonly Hashtable _templates = new();

        const int MaxCacheItems = 1000;
        const int MaxCachedTemplateLength = 1024;

        public MessageTemplate Parse(string messageTemplate)
        {
            if (messageTemplate == null) throw new ArgumentNullException(nameof(messageTemplate));

            if (messageTemplate.Length > MaxCachedTemplateLength)
                return _innerParser.Parse(messageTemplate);

            // ReSharper disable once InconsistentlySynchronizedField
            var result = (MessageTemplate?)_templates[messageTemplate];
            if (result != null)
                return result;

            result = _innerParser.Parse(messageTemplate);

            lock (_templatesLock)
            {
                if (_templates.Count == MaxCacheItems)
                    _templates.Clear();

                _templates[messageTemplate] = result;
            }

            return result;
        }
    }
}
