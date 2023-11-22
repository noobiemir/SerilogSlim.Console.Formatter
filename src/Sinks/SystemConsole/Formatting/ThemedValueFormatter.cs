using System;
using System.IO;
using SerilogSlim.Data;
using SerilogSlim.Events;
using SerilogSlim.Sinks.SystemConsole.Themes;

namespace SerilogSlim.Sinks.SystemConsole.Formatting
{
    abstract class ThemedValueFormatter : LogEventPropertyValueVisitor<ThemedValueFormatterState, int>
    {
        readonly ConsoleTheme _theme;

        protected ThemedValueFormatter(ConsoleTheme theme)
        {
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        }

        protected StyleReset ApplyStyle(TextWriter output, ConsoleThemeStyle style, ref int invisibleCharacterCount)
        {
            return _theme.Apply(output, style, ref invisibleCharacterCount);
        }

        public int Format(LogEventPropertyValue value, TextWriter output, string? format, bool literalTopLevel = false)
        {
            return Visit(new ThemedValueFormatterState { Output = output, Format = format, IsTopLevel = literalTopLevel }, value);
        }

        public abstract ThemedValueFormatter SwitchTheme(ConsoleTheme theme);
    }
}
