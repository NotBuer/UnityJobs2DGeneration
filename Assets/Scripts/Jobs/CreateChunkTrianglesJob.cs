using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkTrianglesJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _chunkCoordIndex;
    [WriteOnly] public NativeArray<int> _chunksTrianglesNativeArray;
    public Mesh.MeshDataArray _chunkMeshDataArray;

    public CreateChunkTrianglesJob(
        ChunkCoord chunkCoord,
        NativeSlice<TileData> tileDataNativeSlice,
        int chunkCoordIndex,
        NativeArray<int> chunksTrianglesNativeArray,
        Mesh.MeshDataArray chunkMeshDataArray)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunkCoordIndex = chunkCoordIndex;
        _chunksTrianglesNativeArray = chunksTrianglesNativeArray;
        _chunkMeshDataArray = chunkMeshDataArray;
    }

    public void Execute()
    {
        _chunkMeshDataArray[_chunkCoordIndex].SetIndexBufferParams(
            World.TRIANGLE_BUFFER_SIZE,
            UnityEngine.Rendering.IndexFormat.UInt32
        );
        NativeArray<int> bufferTrianglesArray = _chunkMeshDataArray[_chunkCoordIndex].GetIndexData<int>();
        int triangleIndexFromStride = _chunkCoordIndex * World.TRIANGLE_BUFFER_SIZE;
        int vertexIndex = 0;
        int triangleArrayIndex = 0;
        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                vertexIndex += Tile.VERTICES;

                _chunksTrianglesNativeArray[triangleIndexFromStride++] = vertexIndex - 4;   // 0
                _chunksTrianglesNativeArray[triangleIndexFromStride++] = vertexIndex - 3;   // 1
                _chunksTrianglesNativeArray[triangleIndexFromStride++] = vertexIndex - 2;   // 2
                _chunksTrianglesNativeArray[triangleIndexFromStride++] = vertexIndex - 4;   // 0
                _chunksTrianglesNativeArray[triangleIndexFromStride++] = vertexIndex - 2;   // 2
                _chunksTrianglesNativeArray[triangleIndexFromStride++] = vertexIndex - 1;   // 3

                bufferTrianglesArray[triangleArrayIndex++] = vertexIndex - 4;   // 0
                bufferTrianglesArray[triangleArrayIndex++] = vertexIndex - 3;   // 1
                bufferTrianglesArray[triangleArrayIndex++] = vertexIndex - 2;   // 2
                bufferTrianglesArray[triangleArrayIndex++] = vertexIndex - 4;   // 0
                bufferTrianglesArray[triangleArrayIndex++] = vertexIndex - 2;   // 2
                bufferTrianglesArray[triangleArrayIndex++] = vertexIndex - 1;   // 3
            }
        }
    }
}
