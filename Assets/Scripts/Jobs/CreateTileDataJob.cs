using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct CreateTileDataJob : IJob
{
    public int chunksInTheWorld;
    public NativeArray<TileData> tileDataNativeArray;

    public void Execute()
    {
        for (int chunkInBuffer = 0; chunkInBuffer <= chunksInTheWorld; chunkInBuffer++)
        {
            int tileInChunk = 0;
            for (int y = 0; y < Chunk.Y_SIZE; y++)
            {
                for (int x = 0; x < Chunk.X_SIZE; x++)
                {
                    tileDataNativeArray[(chunkInBuffer * Chunk.TOTAL_SIZE) + chunkInBuffer + tileInChunk] = new TileData(Tile.TileType.Stone);
                    tileInChunk++;
                }
            }
        }
    }
}