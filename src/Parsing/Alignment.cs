namespace SerilogSlim.Parsing;

internal readonly struct Alignment
{
    public Alignment(AlignmentDirection direction, int width)
    {
        Direction = direction;
        Width = width;
    }

    public AlignmentDirection Direction { get; }

    public int Width { get; }
}