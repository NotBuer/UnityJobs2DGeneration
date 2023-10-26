using System.Numerics;

public static class TextureAtlas
{
    public enum TexCoord : byte
    {
        LEFT_BOTTOM = 0,
        LEFT_TOP = 1,
        RIGHT_TOP = 2,
        RIGHT_BOTTOM = 3
    }

    private const byte SPRITE_CELL_SIZE = 16;

    private const float OFFSET_X = 1.0f / SPRITE_CELL_SIZE;
    private const float OFFSET_Y = 1.0f / SPRITE_CELL_SIZE;

    public static VertexLayout.UVLayout GetUVTextureForTile(TileData tileData, TexCoord texCoord) => tileData.Type switch
    {
        Tile.TileType.Dirt => texCoord switch
        {
            TexCoord.LEFT_BOTTOM => VertexLayout.UVLayout.Default,
            TexCoord.LEFT_TOP => VertexLayout.UVLayout.Default,
            TexCoord.RIGHT_TOP => VertexLayout.UVLayout.Default,
            TexCoord.RIGHT_BOTTOM => VertexLayout.UVLayout.Default,
            _ => VertexLayout.UVLayout.Default,
        },
        Tile.TileType.Grass => texCoord switch
        {
            TexCoord.LEFT_BOTTOM => new VertexLayout.UVLayout(new(0f), new(OFFSET_Y * 15f)),
            TexCoord.LEFT_TOP => new VertexLayout.UVLayout(new(0f), new(1f)),
            TexCoord.RIGHT_TOP => new VertexLayout.UVLayout(new(OFFSET_X), new(1f)),
            TexCoord.RIGHT_BOTTOM => new VertexLayout.UVLayout(new(OFFSET_X), new(OFFSET_Y * 15f)),
            _ => VertexLayout.UVLayout.Default,
        },
        Tile.TileType.Stone => texCoord switch
        {
            TexCoord.LEFT_BOTTOM => new VertexLayout.UVLayout(new(0f), new(OFFSET_Y * 14f)),
            TexCoord.LEFT_TOP => new VertexLayout.UVLayout(new(0f), new(OFFSET_Y * 15f)),
            TexCoord.RIGHT_TOP => new VertexLayout.UVLayout(new(OFFSET_X), new(OFFSET_Y * 15f)),
            TexCoord.RIGHT_BOTTOM => new VertexLayout.UVLayout(new(OFFSET_X), new(OFFSET_Y * 14f)),
            _ => VertexLayout.UVLayout.Default,
        },
        _ => texCoord switch
        {
            TexCoord.LEFT_BOTTOM => VertexLayout.UVLayout.Default,
            TexCoord.LEFT_TOP => VertexLayout.UVLayout.Default,
            TexCoord.RIGHT_TOP => VertexLayout.UVLayout.Default,
            TexCoord.RIGHT_BOTTOM => VertexLayout.UVLayout.Default,
            _ => VertexLayout.UVLayout.Default,
        },
    };

}
