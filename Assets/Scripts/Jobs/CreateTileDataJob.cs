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
        for (ushort index = 0; index < _chunkCoordNativeArray.Length; index++)
        {
            ushort tileInChunk = 0;
            for (byte y = 0; y < Chunk.Y_SIZE; y++)
            {
                for (byte x = 0; x < Chunk.X_SIZE; x++)
                {
                    _tileDataNativeArray[(index * Chunk.TOTAL_SIZE) + tileInChunk] = new TileData(Tile.TileType.Stone);
                    tileInChunk++;
                }
            }
        }
    }
}