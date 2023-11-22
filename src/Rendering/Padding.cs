using System.IO;
using System.Linq;
using SerilogSlim.Parsing;
using System.Text;

namespace SerilogSlim.Rendering;

internal static class Padding
{
    static readonly char[] PaddingChars = Enumerable.Repeat(' ', 80).ToArray();

    public static void Apply(TextWriter output, string value, in Alignment? alignment)
    {
        if (alignment == null || value.Length >= alignment.Value.Width)
        {
            output.Write(value);
            return;
        }

        var pad = alignment.Value.Width - value.Length;

        if (alignment.Value.Direction == AlignmentDirection.Left)
            output.Write(value);

        if (pad <= PaddingChars.Length)
        {
            output.Write(PaddingChars, 0, pad);
        }
        else
        {
            output.Write(new string(' ', pad));
        }

        if (alignment.Value.Direction == AlignmentDirection.Right)
            output.Write(value);
    }

    public static void Apply(TextWriter output, StringBuilder value, in Alignment? alignment)
    {
        if (alignment == null || value.Length >= alignment.Value.Width)
        {
            output.Write(value);
            return;
        }

        var pad = alignment.Value.Width - value.Length;

        if (alignment.Value.Direction == AlignmentDirection.Left)
            output.Write(value);

        if (pad <= PaddingChars.Length)
        {
            output.Write(PaddingChars, 0, pad);
        }
        else
        {
            output.Write(new string(' ', pad));
        }

        if (alignment.Value.Direction == AlignmentDirection.Right)
            output.Write(value);
    }
}