using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct CreateChunkTrianglesJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _chunkCoordIndex;
    [WriteOnly] public NativeArray<int> _chunksTrianglesNativeArray;

    public CreateChunkTrianglesJob(
        ChunkCoord chunkCoord,
        NativeSlice<TileData> tileDataNativeSlice,
        int chunkCoordIndex,
        NativeArray<int> chunksTrianglesNativeArray)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunkCoordIndex = chunkCoordIndex;
        _chunksTrianglesNativeArray = chunksTrianglesNativeArray;
    }

    public void Execute()
    {
        int triangleIndex = _chunkCoordIndex * World.CHUNK_TRIANGLE_ARRAY_LENGTH;
        int vertexIndex = 0;
        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                vertexIndex += Tile.VERTICES;
                _chunksTrianglesNativeArray[triangleIndex++] = vertexIndex - 4;   // 0
                _chunksTrianglesNativeArray[triangleIndex++] = vertexIndex - 3;   // 1
                _chunksTrianglesNativeArray[triangleIndex++] = vertexIndex - 2;   // 2
                _chunksTrianglesNativeArray[triangleIndex++] = vertexIndex - 4;   // 0
                _chunksTrianglesNativeArray[triangleIndex++] = vertexIndex - 2;   // 2
                _chunksTrianglesNativeArray[triangleIndex++] = vertexIndex - 1;   // 3
            }
        }
    }
}
