using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct CreateTileDataJob : IJob
{
    [ReadOnly] public NativeArray<ChunkCoord> _chunkCoordNativeArray;
    public NativeArray<TileData> _tileDataNativeArray;

    public CreateTileDataJob(NativeArray<ChunkCoord> chunkCoordNativeArray, NativeArray<TileData> tileDataNativeArray)
    {
        _chunkCoordNativeArray = chunkCoordNativeArray;
        _tileDataNativeArray = tileDataNativeArray;
    }

    public void Execute()
    {
        for (int chunkInBuffer = 0; chunkInBuffer <= _chunkCoordNativeArray.Length - 1; chunkInBuffer++)
        {
            int tileInChunk = 0;
            for (int y = 0; y < Chunk.Y_SIZE; y++)
            {
                for (int x = 0; x < Chunk.X_SIZE; x++)
                {
                    _tileDataNativeArray[(chunkInBuffer * Chunk.TOTAL_SIZE) + chunkInBuffer + tileInChunk] = new TileData(Tile.TileType.Stone);
                    tileInChunk++;
                }
            }
        }
    }
}