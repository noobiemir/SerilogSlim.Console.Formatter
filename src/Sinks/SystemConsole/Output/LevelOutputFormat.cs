﻿using System;
using Microsoft.Extensions.Logging;

namespace SerilogSlim.Sinks.SystemConsole.Output;

internal static class LevelOutputFormat
{
    static readonly string[][] TitleCaseLevelMap = {
        new []{ "V", "Vb", "Vrb", "Verb", "Verbo", "Verbos", "Verbose" },
        new []{ "D", "De", "Dbg", "Dbug", "Debug" },
        new []{ "I", "In", "Inf", "Info", "Infor", "Inform", "Informa", "Informat", "Informati", "Informatio", "Information" },
        new []{ "W", "Wn", "Wrn", "Warn", "Warni", "Warnin", "Warning" },
        new []{ "E", "Er", "Err", "Eror", "Error" },
        new []{ "F", "Fa", "Ftl", "Fatl", "Fatal" }
    };

    static readonly string[][] LowerCaseLevelMap = {
        new []{ "v", "vb", "vrb", "verb", "verbo", "verbos", "verbose" },
        new []{ "d", "de", "dbg", "dbug", "debug" },
        new []{ "i", "in", "inf", "info", "infor", "inform", "informa", "informat", "informati", "informatio", "information" },
        new []{ "w", "wn", "wrn", "warn", "warni", "warnin", "warning" },
        new []{ "e", "er", "err", "eror", "error" },
        new []{ "f", "fa", "ftl", "fatl", "fatal" }
    };

    static readonly string[][] UpperCaseLevelMap = {
        new []{ "V", "VB", "VRB", "VERB", "VERBO", "VERBOS", "VERBOSE" },
        new []{ "D", "DE", "DBG", "DBUG", "DEBUG" },
        new []{ "I", "IN", "INF", "INFO", "INFOR", "INFORM", "INFORMA", "INFORMAT", "INFORMATI", "INFORMATIO", "INFORMATION" },
        new []{ "W", "WN", "WRN", "WARN", "WARNI", "WARNIN", "WARNING" },
        new []{ "E", "ER", "ERR", "EROR", "ERROR" },
        new []{ "F", "FA", "FTL", "FATL", "FATAL" }
    };

    public static string Format(string value, string? format = null)
    {
        return format switch
        {
            "u" => value.ToUpperInvariant(),
            "w" => value.ToLowerInvariant(),
            _ => value
        };
    }

    public static string GetLevelMoniker(LogLevel value, string? format = null)
    {
        var index = (int)value;
        if (index is < 0 or > (int)LogLevel.Critical)
            return Format(value.ToString(), format);

        if (format == null || format.Length != 2 && format.Length != 3)
            return Format(GetLevelMoniker(TitleCaseLevelMap, index), format);

        // Using int.Parse() here requires allocating a string to exclude the first character prefix.
        // Junk like "wxy" will be accepted but produce benign results.
        var width = format[1] - '0';
        if (format.Length == 3)
        {
            width *= 10;
            width += format[2] - '0';
        }

        if (width < 1)
            return string.Empty;

        switch (format[0])
        {
            case 'w':
                return GetLevelMoniker(LowerCaseLevelMap, index, width);
            case 'u':
                return GetLevelMoniker(UpperCaseLevelMap, index, width);
            case 't':
                return GetLevelMoniker(TitleCaseLevelMap, index, width);
            default:
                return Format(GetLevelMoniker(TitleCaseLevelMap, index), format);
        }
    }

    static string GetLevelMoniker(string[][] caseLevelMap, int index, int width)
    {
        var caseLevel = caseLevelMap[index];
        return caseLevel[Math.Min(width, caseLevel.Length) - 1];
    }

    static string GetLevelMoniker(string[][] caseLevelMap, int index)
    {
        var caseLevel = caseLevelMap[index];
        return caseLevel[^1];
    }
}