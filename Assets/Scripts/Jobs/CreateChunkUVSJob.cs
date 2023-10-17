using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkUVSJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _stride;
    [WriteOnly] public NativeArray<Vector2> _chunksUVSNativeArray;

    public CreateChunkUVSJob(
        ChunkCoord chunkCoord,
        NativeSlice<TileData> tileDataNativeSlice,
        int stride,
        NativeArray<Vector2> chunksUVSNativeArray)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _stride = stride;
        _chunksUVSNativeArray = chunksUVSNativeArray;
    }

    public void Execute()
    {
        short uvIndex = 0;
        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                _chunksUVSNativeArray[uvIndex++] = new Vector2(0, 0);
                _chunksUVSNativeArray[uvIndex++] = new Vector2(0, 1);
                _chunksUVSNativeArray[uvIndex++] = new Vector2(1, 1);
                _chunksUVSNativeArray[uvIndex++] = new Vector2(1, 0);
            }
        }
    }
}
