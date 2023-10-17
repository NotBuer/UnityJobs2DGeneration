using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkVerticesJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _stride;
    [WriteOnly] public NativeArray<Vector3Int> _chunksVerticesNativeArray;

    public CreateChunkVerticesJob(
        ChunkCoord chunkCoord, 
        NativeSlice<TileData> tileDataNativeSlice, 
        int stride, 
        NativeArray<Vector3Int> chunksVerticesNativeArray)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _stride = stride;
        _chunksVerticesNativeArray = chunksVerticesNativeArray;
    }

    public void Execute()
    {
        short verticeIndex = 0;
        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                _chunksVerticesNativeArray[verticeIndex++] = new Vector3Int(x, y);
                _chunksVerticesNativeArray[verticeIndex++] = new Vector3Int(x, 1 + y);
                _chunksVerticesNativeArray[verticeIndex++] = new Vector3Int(x + 1, y + 1);
                _chunksVerticesNativeArray[verticeIndex++] = new Vector3Int(x + 1, y);
            }
        }
    }
}
