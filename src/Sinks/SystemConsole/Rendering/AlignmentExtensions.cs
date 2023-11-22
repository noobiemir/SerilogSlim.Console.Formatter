using SerilogSlim.Parsing;

namespace SerilogSlim.Sinks.SystemConsole.Rendering;

static class AlignmentExtensions
{
    public static Alignment Widen(this Alignment alignment, int amount)
    {
        return new Alignment(alignment.Direction, alignment.Width + amount);
    }
}