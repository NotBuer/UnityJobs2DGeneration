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

    public readonly bool Equals(ChunkCoord other) =>
        (other.XCoord == XCoord && other.YCoord == YCoord);

    public readonly override bool Equals(object obj)
    {
        if (obj is not ChunkCoord) return false;
        return Equals(obj);
    }

    public readonly override int GetHashCode() =>
        XCoord ^ YCoord;

    public static bool operator ==(ChunkCoord lhs, ChunkCoord rhs) =>
        lhs.Equals(rhs);

    public static bool operator !=(ChunkCoord lhs, ChunkCoord rhs) =>
        !lhs.Equals(rhs);
}
