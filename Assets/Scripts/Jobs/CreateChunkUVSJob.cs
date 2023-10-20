using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public struct CreateChunkUVSJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _chunkCoordIndex;
    [WriteOnly] public NativeArray<Vector2> _chunksUVSNativeArray;
    public Mesh.MeshDataArray _chunkMeshDataArray;
    [ReadOnly] public NativeArray<VertexAttributeDescriptor> _uvLayout;

    public CreateChunkUVSJob(
        ChunkCoord chunkCoord,
        NativeSlice<TileData> tileDataNativeSlice,
        int chunkCoordIndex,
        NativeArray<Vector2> chunksUVSNativeArray,
        Mesh.MeshDataArray chunkMeshDataArray,
        NativeArray<VertexAttributeDescriptor> uvLayout)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunkCoordIndex = chunkCoordIndex;
        _chunksUVSNativeArray = chunksUVSNativeArray;
        _chunkMeshDataArray = chunkMeshDataArray;
        _uvLayout = uvLayout;
    }

    public void Execute()
    {
        _chunkMeshDataArray[_chunkCoordIndex].SetVertexBufferParams(
            World.VERTEX_BUFFER_SIZE + World.UV_BUFFER_SIZE,
            _uvLayout
        );
        NativeArray<Vector2> bufferUVArray = _chunkMeshDataArray[_chunkCoordIndex].GetVertexData<Vector2>();
        int uvIndexFromStride = _chunkCoordIndex * World.UV_BUFFER_SIZE;
        int uvArrayIndex = World.VERTEX_BUFFER_SIZE;
        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(0, 0);   //(0, 0)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(0, 1);   //(0, 1)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(1, 1);   //(1, 1)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(1, 0);   //(1, 0)

                bufferUVArray[uvArrayIndex++] = new Vector2(0, 0);   //(0, 0)
                bufferUVArray[uvArrayIndex++] = new Vector2(0, 1);   //(0, 1)
                bufferUVArray[uvArrayIndex++] = new Vector2(1, 1);   //(1, 1)
                bufferUVArray[uvArrayIndex++] = new Vector2(1, 0);   //(1, 0)
            }
        }
    }
}
