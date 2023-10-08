using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct CreateChunksJob : IJob
{
    public short worldMiddleX;
    public short worldMiddleY;
    public NativeHashMap<ChunkCoord, ChunkData> chunksHashMap;

    public void Execute()
    {
        for (short y = (short)-worldMiddleY; y <= worldMiddleY; y++)
        {
            for (short x = (short)-worldMiddleX; x <= worldMiddleX; x++)
            {
                ChunkCoord coord = new ChunkCoord(x, y);
                chunksHashMap.Add(coord, new ChunkData());
            }
        }
    }
}
