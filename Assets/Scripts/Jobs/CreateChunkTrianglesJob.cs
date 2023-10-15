using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkTrianglesJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _stride;
    [WriteOnly] public NativeArray<int> _worldChunksTrianglesNativeArray;

    public CreateChunkTrianglesJob(
        ChunkCoord chunkCoord,
        NativeSlice<TileData> tileDataNativeSlice,
        int stride,
        NativeArray<int> worldChunksTrianglesNativeArray)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _stride = stride;
        _worldChunksTrianglesNativeArray = worldChunksTrianglesNativeArray;
    }
    public void Execute()
    {
        int verticesBuffer = (Chunk.TOTAL_SIZE * Tile.TRIANGLES);
        int start = _stride * verticesBuffer;
        for (int i = start; i < verticesBuffer; i += Tile.TRIANGLES)
        {
            _worldChunksTrianglesNativeArray[i + 0] = 0;
            _worldChunksTrianglesNativeArray[i + 1] = 1;
            _worldChunksTrianglesNativeArray[i + 2] = 2;
            _worldChunksTrianglesNativeArray[i + 3] = 0;
            _worldChunksTrianglesNativeArray[i + 4] = 2;
            _worldChunksTrianglesNativeArray[i + 5] = 3;
        }
    }
}
