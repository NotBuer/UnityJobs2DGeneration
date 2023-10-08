using Unity.Collections;

public struct ChunkData
{
    public ChunkCoord ChunkCoord { get; set; }
    public NativeHashMap<TileCoord, TileData> TilesHashMap { get; set; }

    public ChunkData(ChunkCoord chunkCoord, NativeHashMap<TileCoord, TileData> tilesHashMap)
    {
        ChunkCoord = chunkCoord;
        TilesHashMap = tilesHashMap;
    }
}
