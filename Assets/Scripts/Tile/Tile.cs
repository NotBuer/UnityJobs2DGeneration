public static class Tile
{
    public const byte VERTICES = 4;
    public const byte UVS = 4;
    public const byte TRIANGLES = 6;

    public enum TileType : byte
    {
        Air = 0,
        Dirt = 1,
        Grass = 2,
        Stone = 3,
    }
}
