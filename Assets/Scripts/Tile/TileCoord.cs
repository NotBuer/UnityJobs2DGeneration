using System;
using System.Runtime.InteropServices;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct TileCoord : IEquatable<TileCoord>
{
    public int XCoord { get; set; }
    public int YCoord { get; set; }

    public TileCoord(int xCoord, int yCoord)
    {
        XCoord = xCoord;
        YCoord = yCoord;
    }

    public readonly bool Equals(TileCoord other) =>
        (other.XCoord == XCoord && other.YCoord == YCoord);

    public readonly override bool Equals(object obj)
    {
        if (obj is not TileCoord) return false;
        return Equals(obj);
    }

    public readonly override int GetHashCode() =>
        XCoord ^ YCoord;

    public static bool operator ==(TileCoord lhs, TileCoord rhs) =>
        lhs.Equals(rhs);

    public static bool operator !=(TileCoord lhs, TileCoord rhs) =>
        !lhs.Equals(rhs);
}
