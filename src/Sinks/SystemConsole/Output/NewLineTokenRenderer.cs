using System;
using System.IO;
using SerilogSlim.Events;
using SerilogSlim.Parsing;
using SerilogSlim.Rendering;
using SerilogSlim.Sinks.SystemConsole.Rendering;

namespace SerilogSlim.Sinks.SystemConsole.Output
{
    internal class NewLineTokenRenderer : OutputTemplateTokenRenderer
    {
        readonly Alignment? _alignment;

        public NewLineTokenRenderer(Alignment? alignment)
        {
            _alignment = alignment;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            if (_alignment.HasValue)
                Padding.Apply(output, Environment.NewLine, _alignment.Value.Widen(Environment.NewLine.Length));
            else
                output.WriteLine();
        }
    }
}
