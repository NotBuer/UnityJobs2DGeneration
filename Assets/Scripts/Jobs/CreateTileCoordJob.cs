using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct CreateTileCoordJob : IJob
{
    [ReadOnly] public NativeArray<ChunkCoord> _chunkCoordNativeArray;
    public NativeArray<TileCoord> _tileCoordNativeArray;

    public CreateTileCoordJob(NativeArray<ChunkCoord> chunkCoordNativeArray, NativeArray<TileCoord> tileCoordNativeArray)
    {
        _chunkCoordNativeArray = chunkCoordNativeArray;
        _tileCoordNativeArray = tileCoordNativeArray;
    }

    public void Execute()
    {
        for (int stride = 0; stride <= _chunkCoordNativeArray.Length - 1; stride++)
        {
            int tileInChunk = 0;
            for (int y = 0; y < Chunk.Y_SIZE; y++)
            {
                for (int x = 0; x < Chunk.X_SIZE; x++)
                {
                    _tileCoordNativeArray[(stride * Chunk.TOTAL_SIZE) + stride + tileInChunk] = new TileCoord(x, y);
                    tileInChunk++;
                }
            }
        }
    }
}