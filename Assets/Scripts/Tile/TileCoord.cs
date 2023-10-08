using UnityEngine;

public struct TileCoord
{
    public int XCoord { get; set; }
    public int YCoord { get; set; }

    public TileCoord(int xCoord, int yCoord)
    {
        XCoord = xCoord;
        YCoord = yCoord;
    }
}
