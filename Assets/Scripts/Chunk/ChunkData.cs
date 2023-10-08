using Unity.Collections;

public struct ChunkData
{
    public ChunkCoord ChunkCoord { get; set; }

    public ChunkData(ChunkCoord chunkCoord)
    {
        ChunkCoord = chunkCoord;
    }
}
