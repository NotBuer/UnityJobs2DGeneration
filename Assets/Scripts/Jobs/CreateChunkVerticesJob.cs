using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public struct CreateChunkVerticesJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _chunkCoordIndex;
    [WriteOnly] public NativeArray<Vector3> _chunksVerticesNativeArray;
    public Mesh.MeshDataArray _chunkMeshDataArray;
    [ReadOnly] NativeArray<VertexAttributeDescriptor> _layout;
    public bool _useAdvancedMeshAPI;

    public CreateChunkVerticesJob(
        ChunkCoord chunkCoord, 
        NativeSlice<TileData> tileDataNativeSlice, 
        int chunkCoordIndex, 
        NativeArray<Vector3> chunksVerticesNativeArray,
        Mesh.MeshDataArray chunkMeshDataArray,
        NativeArray<VertexAttributeDescriptor> layout,
        bool useAdvancedMeshAPI)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunkCoordIndex = chunkCoordIndex;
        _chunksVerticesNativeArray = chunksVerticesNativeArray;
        _chunkMeshDataArray = chunkMeshDataArray;
        _layout = layout;
        _useAdvancedMeshAPI = useAdvancedMeshAPI;
    }

    public void Execute()
    {
        // TIP: Is possible to pass the '_chunkcoord' coordinates incrementing the vertex position,
        // converting all chunk tiles from Local Position to World Space Position.

        NativeArray<VertexLayout> bufferVertexArray;

        if (_useAdvancedMeshAPI)
        {
            //_chunkMeshDataArray[_chunkCoordIndex].SetVertexBufferParams(VertexLayout.VERTEX_BUFFER_SIZE, _layout);
            bufferVertexArray = _chunkMeshDataArray[_chunkCoordIndex].GetVertexData<VertexLayout>();
        } 
        else bufferVertexArray = new NativeArray<VertexLayout>(0, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        int vertexIndexFromStride = _chunkCoordIndex * VertexLayout.VERTEX_BUFFER_SIZE;
        int vertexArrayIndex = 0;
        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                if (_useAdvancedMeshAPI)
                {
                    VertexLayout LeftBottomCoord = bufferVertexArray[vertexArrayIndex];
                    LeftBottomCoord._position = new Vector3(x, y);
                    bufferVertexArray[vertexArrayIndex + 0] = LeftBottomCoord;

                    VertexLayout LeftTopCoord = bufferVertexArray[vertexArrayIndex];
                    LeftTopCoord._position = new Vector3(x, 1 + y);
                    bufferVertexArray[vertexArrayIndex + 1] = LeftTopCoord;

                    VertexLayout RigthTopCoord = bufferVertexArray[vertexArrayIndex];
                    RigthTopCoord._position = new Vector3(x + 1, y + 1);
                    bufferVertexArray[vertexArrayIndex + 2] = RigthTopCoord;

                    VertexLayout RigthBottomCoord = bufferVertexArray[vertexArrayIndex];
                    RigthBottomCoord._position = new Vector3(x + 1, y);
                    bufferVertexArray[vertexArrayIndex + 3] = RigthBottomCoord;

                    //bufferVertexArray[vertexArrayIndex++] = new VertexLayout { _position = new Vector3(x, y) };         //(0, 0)
                    //bufferVertexArray[vertexArrayIndex++] = new VertexLayout { _position = new Vector3(x, 1 + y) };;    //(0, 1)
                    //bufferVertexArray[vertexArrayIndex++] = new VertexLayout { _position = new Vector3(x + 1, y + 1) }; //(1, 1)
                    //bufferVertexArray[vertexArrayIndex++] = new VertexLayout { _position = new Vector3(x + 1, y) };     //(1, 0)

                    //bufferVertexArray[vertexArrayIndex++] = new Vector3(x, y);          //(0, 0)
                    //bufferVertexArray[vertexArrayIndex++] = new Vector3(x, 1 + y);      //(0, 1)
                    //bufferVertexArray[vertexArrayIndex++] = new Vector3(x + 1, y + 1);  //(1, 1)
                    //bufferVertexArray[vertexArrayIndex++] = new Vector3(x + 1, y);      //(1, 0)
                    continue;
                }

                _chunksVerticesNativeArray[vertexIndexFromStride++] = new Vector3(x, y);          //(0, 0)
                _chunksVerticesNativeArray[vertexIndexFromStride++] = new Vector3(x, 1 + y);      //(0, 1)
                _chunksVerticesNativeArray[vertexIndexFromStride++] = new Vector3(x + 1, y + 1);  //(1, 1)
                _chunksVerticesNativeArray[vertexIndexFromStride++] = new Vector3(x + 1, y);      //(1, 0)
            }
        }

        if (!_useAdvancedMeshAPI) bufferVertexArray.Dispose();
    }
}
