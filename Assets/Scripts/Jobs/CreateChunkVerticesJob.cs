using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkVerticesJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _chunkCoordIndex;
    [WriteOnly] public NativeArray<Vector3> _chunksVerticesNativeArray;
    public Mesh.MeshDataArray _chunkMeshDataArray;

    public CreateChunkVerticesJob(
        ChunkCoord chunkCoord, 
        NativeSlice<TileData> tileDataNativeSlice, 
        int chunkCoordIndex, 
        NativeArray<Vector3> chunksVerticesNativeArray,
        Mesh.MeshDataArray chunkMeshDataArray)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunkCoordIndex = chunkCoordIndex;
        _chunksVerticesNativeArray = chunksVerticesNativeArray;
        _chunkMeshDataArray = chunkMeshDataArray;
    }

    public void Execute()
    {
        // TIP: Is possible to pass the '_chunkcoord' coordinates incrementing the vertex position,
        // converting all chunk tiles from Local Position to World Space Position.

        NativeArray<Vector3> rawBufferPointerData = _chunkMeshDataArray[_chunkCoordIndex].GetVertexData<Vector3>();
        int vertexIndexFromStride = _chunkCoordIndex * World.CHUNK_VERTEX_ARRAY_LENGTH;
        int index = 0;
        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                _chunksVerticesNativeArray[vertexIndexFromStride++] = new Vector3(x, y);          //(0, 0)
                _chunksVerticesNativeArray[vertexIndexFromStride++] = new Vector3(x, 1 + y);      //(0, 1)
                _chunksVerticesNativeArray[vertexIndexFromStride++] = new Vector3(x + 1, y + 1);  //(1, 1)
                _chunksVerticesNativeArray[vertexIndexFromStride++] = new Vector3(x + 1, y);      //(1, 0)

                rawBufferPointerData[index++] = new Vector3(x, y);          //(0, 0)
                rawBufferPointerData[index++] = new Vector3(x, 1 + y);      //(0, 1)
                rawBufferPointerData[index++] = new Vector3(x + 1, y + 1);  //(1, 1)
                rawBufferPointerData[index++] = new Vector3(x + 1, y);      //(1, 0)
            }
        }
    }
}
