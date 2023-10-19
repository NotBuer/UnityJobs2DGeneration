using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkUVSJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _chunkCoordIndex;
    [WriteOnly] public NativeArray<Vector2> _chunksUVSNativeArray;
    public Mesh.MeshDataArray _chunkMeshDataArray;

    public CreateChunkUVSJob(
        ChunkCoord chunkCoord,
        NativeSlice<TileData> tileDataNativeSlice,
        int chunkCoordIndex,
        NativeArray<Vector2> chunksUVSNativeArray,
        Mesh.MeshDataArray chunkMeshDataArray)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunkCoordIndex = chunkCoordIndex;
        _chunksUVSNativeArray = chunksUVSNativeArray;
        _chunkMeshDataArray = chunkMeshDataArray;
    }

    public void Execute()
    {
        NativeArray<Vector2> rawBufferPointerData = _chunkMeshDataArray[_chunkCoordIndex].GetVertexData<Vector2>();
        int uvIndexFromStride = _chunkCoordIndex * World.CHUNK_UV_ARRAY_LENGTH;
        int index = 1024;
        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(0, 0);   //(0, 0)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(0, 1);   //(0, 1)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(1, 1);   //(1, 1)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(1, 0);   //(1, 0)

                rawBufferPointerData[index++] = new Vector2(0, 0);   //(0, 0)
                rawBufferPointerData[index++] = new Vector2(0, 1);   //(0, 1)
                rawBufferPointerData[index++] = new Vector2(1, 1);   //(1, 1)
                rawBufferPointerData[index++] = new Vector2(1, 0);   //(1, 0)
            }
        }
    }
}
