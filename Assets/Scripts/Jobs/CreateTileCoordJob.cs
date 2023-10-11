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
        for(int chunkInBuffer = 0; chunkInBuffer <= chunksInTheWorld; chunkInBuffer++)
        {
            int tileInChunk = 0;
            for (int y = 0; y < Chunk.Y_SIZE; y++)
            {
                for (int x = 0; x < Chunk.X_SIZE; x++)
                {
                    tileCoordNativeArray[(chunkInBuffer * Chunk.TOTAL_SIZE) + chunkInBuffer + tileInChunk] = new TileCoord(x, y);
                    tileInChunk++;
                }
            }
        }
    }
}