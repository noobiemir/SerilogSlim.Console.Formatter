using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SerilogSlim.Sinks.SystemConsole.Themes
{
    public class AnsiConsoleTheme : ConsoleTheme
    {
        /// <summary>
        /// A 256-color theme along the lines of Visual Studio Code.
        /// </summary>
        public static AnsiConsoleTheme Code { get; } = AnsiConsoleThemes.Code;

        /// <summary>
        /// A theme using only gray, black and white.
        /// </summary>
        public static AnsiConsoleTheme Grayscale { get; } = AnsiConsoleThemes.Grayscale;

        /// <summary>
        /// A theme in the style of the original <i>SerilogSlim.Sinks.Literate</i>.
        /// </summary>
        public static AnsiConsoleTheme Literate { get; } = AnsiConsoleThemes.Literate;

        /// <summary>
        /// A theme in the style of the original <i>SerilogSlim.Sinks.Literate</i> using only standard 16 terminal colors that will work on light backgrounds.
        /// </summary>
        public static AnsiConsoleTheme Sixteen { get; } = AnsiConsoleThemes.Sixteen;

        readonly IReadOnlyDictionary<ConsoleThemeStyle, string> _styles;
        const string AnsiStyleReset = "\x1b[0m";

        public AnsiConsoleTheme(IReadOnlyDictionary<ConsoleThemeStyle, string> styles)
        {
            if (styles is null) throw new ArgumentNullException(nameof(styles));
            _styles = styles.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public override bool CanBuffer => true;

        public override int Set(TextWriter output, ConsoleThemeStyle style)
        {
            if (_styles.TryGetValue(style, out var ansiStyle))
            {
                output.Write(ansiStyle);
                return ansiStyle.Length;
            }
            return 0;
        }

        public override void Reset(TextWriter output)
        {
            output.Write(AnsiStyleReset);
        }

        protected override int ResetCharCount => AnsiStyleReset.Length;
    }
}
