using SerilogSlim.Events;

namespace SerilogSlim.Parsing;

internal interface IMessageTemplateParser
{
    MessageTemplate Parse(string messageTemplate);
}