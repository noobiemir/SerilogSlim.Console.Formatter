using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SerilogSlim;
using SerilogSlim.Capturing;
using SerilogSlim.Core;
using SerilogSlim.Formatting;
using SerilogSlim.Sinks.SystemConsole.Output;
using SerilogSlim.Sinks.SystemConsole.Platform;
using SerilogSlim.Sinks.SystemConsole.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Logging;

public static class ConsoleLoggerExtensions
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050")]
    public static ILoggingBuilder AddColorConsole(
        this ILoggingBuilder builder,
        string outputTemplate = ColorConsoleFormatterOptions.DefaultConsoleOutputTemplate,
        IFormatProvider? formatProvider = null,
        ConsoleTheme? theme = null,
        bool applyThemeToRedirectedOutput = false)
    {
        WindowsConsole.EnableVirtualTerminalProcessing();
        builder.AddConsole(options =>
        {
            options.FormatterName = ColorConsoleFormatter.FormatterName;
        });

        var appliedTheme = !applyThemeToRedirectedOutput && (System.Console.IsOutputRedirected || System.Console.IsErrorRedirected) ?
            ConsoleTheme.None :
            theme ?? AnsiConsoleThemes.Code;
        var formatter = new OutputTemplateRenderer(appliedTheme, outputTemplate, formatProvider);

        builder.Services.AddSingleton<ITextFormatter>(formatter);

        var converter = new PropertyValueConverter(
            10,
            int.MaxValue,
            int.MaxValue,
            new List<Type>(),
            new List<Type>(),
            new List<IDestructuringPolicy>(),
            false);

        builder.Services.AddSingleton(converter);

        builder.AddConsoleFormatter<ColorConsoleFormatter, ColorConsoleFormatterOptions>();

        return builder;
    }
}