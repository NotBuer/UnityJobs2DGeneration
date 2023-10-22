using System.Numerics;

public static class SpriteAtlasUtils
{

    public const ushort ATLAS_WIDTH = 256;
    public const ushort ATLAS_HEIGHT = 256;

    private const byte SPRITE_CELL_SIZE = 16;

    private const float TEX_COORDX_OFFSET = 1.0f / SPRITE_CELL_SIZE;
    private const float TEX_COORDY_OFFSET = 1.0f / SPRITE_CELL_SIZE;

    public static Vector2 GetUVTextureFromAtlas(Vector2 coord)
    {
        return new Vector2();
    }

    public static VertexLayout.UVLayoutWrapper GetUVTextureForTile(TileData tileData) 
    {
        switch (tileData.Type)
        {
            // TODO...
            case Tile.TileType.Dirt: return VertexLayout.UVLayoutWrapper.Default;

            case Tile.TileType.Grass: return new VertexLayout.UVLayoutWrapper(TEX_COORDX_OFFSET, );

            // TODO...
            case Tile.TileType.Stone: return VertexLayout.UVLayoutWrapper.Default;

            default:
                return VertexLayout.UVLayoutWrapper.Default;
        }
    }

}
