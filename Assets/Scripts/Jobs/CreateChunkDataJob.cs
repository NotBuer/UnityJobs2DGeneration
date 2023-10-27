using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct CreateChunkDataJob : IJob
{
    [ReadOnly] public NativeArray<ChunkCoord> _chunkCoordNativeArray;
    [WriteOnly] public NativeArray<TileCoord> _tileCoordNativeArray;
    [WriteOnly] public NativeArray<TileData> _tileDataNativeArray;

    public CreateChunkDataJob(
        NativeArray<ChunkCoord> chunkCoordNativeArray, 
        NativeArray<TileCoord> tileCoordNativeArray, 
        NativeArray<TileData> tileDataNativeArray)
    {
        _chunkCoordNativeArray = chunkCoordNativeArray;
        _tileCoordNativeArray = tileCoordNativeArray;
        _tileDataNativeArray = tileDataNativeArray;
    }

    public void Execute()
    {
        for (ushort i = 0; i < _chunkCoordNativeArray.Length; i++)
        {
            ushort tileInChunk = 0;
            for (byte y = 0; y < Chunk.Y_SIZE; y++)
            {
                for (byte x = 0; x < Chunk.X_SIZE; x++)
                {
                    int location = (i * Chunk.TOTAL_SIZE) + tileInChunk;
                    _tileCoordNativeArray[location] = new TileCoord(x , y);
                    // TODO... Perlin noise height
                    _tileDataNativeArray[location] = new TileData(Tile.TileType.Stone);
                    tileInChunk++;
                }
            }
        }
    }
}
