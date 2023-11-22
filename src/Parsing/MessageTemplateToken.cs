using System;
using System.Collections.Generic;
using System.IO;
using SerilogSlim.Events;

namespace SerilogSlim.Parsing;

internal abstract class MessageTemplateToken
{
    public abstract int Length { get; }

    public abstract void Render(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider? formatProvider = null);
}