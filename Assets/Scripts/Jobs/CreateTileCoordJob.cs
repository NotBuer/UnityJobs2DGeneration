using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct CreateTileCoordJob : IJob
{
    public int chunksInTheWorld;
    public NativeArray<TileCoord> tileCoordNativeArray;

    public void Execute()
    {
        for (int stride = 0; stride <= chunksInTheWorld; stride++)
        {
            int tileInChunk = 0;
            for (int y = 0; y < Chunk.Y_SIZE; y++)
            {
                for (int x = 0; x < Chunk.X_SIZE; x++)
                {
                    tileCoordNativeArray[(stride * Chunk.TOTAL_SIZE) + stride + tileInChunk] = new TileCoord(x, y);
                    tileInChunk++;
                }
            }
        }
    }
}