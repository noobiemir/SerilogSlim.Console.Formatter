using System;
using System.IO;

namespace SerilogSlim.Debugging;

internal static class SelfLog
{
    static Action<string>? _output;

    public static void Enable(TextWriter output)
    {
        if (output == null) throw new ArgumentNullException(nameof(output));

        Enable(m =>
        {
            output.WriteLine(m);
            output.Flush();
        });
    }

    public static void Enable(Action<string> output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public static void Disable()
    {
        _output = null;
    }

    public static void WriteLine(string format, object? arg0 = null, object? arg1 = null, object? arg2 = null)
    {
        _output?.Invoke(string.Format(DateTime.UtcNow.ToString("o") + " " + format, arg0, arg1, arg2));
    }
}