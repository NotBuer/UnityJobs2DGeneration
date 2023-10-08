using System;

public struct ChunkCoord : IEquatable<ChunkCoord>
{
    public int XCoord { get; set; }
    public int YCoord { get; set; }

    public ChunkCoord(int xCoord, int yCoord)
    {
        XCoord = xCoord;
        YCoord = yCoord;
    }

    public readonly bool Equals(ChunkCoord other)
    {
        return (other.XCoord == XCoord && other.YCoord == YCoord);
    }
}
