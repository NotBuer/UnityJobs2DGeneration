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
    [ReadOnly] public NativeArray<VertexAttributeDescriptor> _layout;
    //public NativeArray<Vector2> _bufferUVArray;
    public bool _useAdvancedMeshAPI;

    public CreateChunkUVSJob(
        ChunkCoord chunkCoord,
        NativeSlice<TileData> tileDataNativeSlice,
        int chunkCoordIndex,
        NativeArray<Vector2> chunksUVSNativeArray,
        Mesh.MeshDataArray chunkMeshDataArray,
        NativeArray<VertexAttributeDescriptor> layout,
        //NativeArray<Vector2> bufferUVArray,
        bool useAdvancedMeshAPI)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunkCoordIndex = chunkCoordIndex;
        _chunksUVSNativeArray = chunksUVSNativeArray;
        _chunkMeshDataArray = chunkMeshDataArray;
        _layout = layout;
        //_bufferUVArray = bufferUVArray;
        _useAdvancedMeshAPI = useAdvancedMeshAPI;
    }

    public void Execute()
    {
        NativeArray<VertexLayout> bufferUVArray;

        if (_useAdvancedMeshAPI)
        {
            //_chunkMeshDataArray[_chunkCoordIndex].SetVertexBufferParams(VertexLayout.VERTEX_BUFFER_SIZE, _layout);
            bufferUVArray = _chunkMeshDataArray[_chunkCoordIndex].GetVertexData<VertexLayout>();
        }
        else bufferUVArray = new NativeArray<VertexLayout>(0, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        int uvIndexFromStride = _chunkCoordIndex * VertexLayout.VERTEX_BUFFER_SIZE;
        int uvArrayIndex = 0;

        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                if (_useAdvancedMeshAPI)
                {
                    VertexLayout LeftBottomCoord = bufferUVArray[uvArrayIndex];
                    LeftBottomCoord._texCoordX = 0;
                    LeftBottomCoord._textCoordY = 0;
                    bufferUVArray[uvArrayIndex + 0] = LeftBottomCoord;

                    VertexLayout LeftTopCoord = bufferUVArray[uvArrayIndex];
                    LeftTopCoord._texCoordX = 0;
                    LeftTopCoord._textCoordY = 1;
                    bufferUVArray[uvArrayIndex + 1] = LeftTopCoord;

                    VertexLayout RigthTopCoord = bufferUVArray[uvArrayIndex];
                    RigthTopCoord._texCoordX = 1;
                    RigthTopCoord._textCoordY = 1;
                    bufferUVArray[uvArrayIndex + 2] = RigthTopCoord;

                    VertexLayout RigthBottomCoord = bufferUVArray[uvArrayIndex];
                    RigthBottomCoord._texCoordX = 1;
                    RigthBottomCoord._textCoordY = 0;
                    bufferUVArray[uvArrayIndex + 3] = RigthBottomCoord;

                    uvArrayIndex += Tile.UVS;

                    //bufferUVArray[uvArrayIndex++] = new Vector2(0, 0);   //(0, 0)
                    //bufferUVArray[uvArrayIndex++] = new Vector2(0, 1);   //(0, 1)
                    //bufferUVArray[uvArrayIndex++] = new Vector2(1, 1);   //(1, 1)
                    //bufferUVArray[uvArrayIndex++] = new Vector2(1, 0);   //(1, 0)
                    continue;
                }

                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(0, 0);   //(0, 0)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(0, 1);   //(0, 1)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(1, 1);   //(1, 1)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(1, 0);   //(1, 0)
            }
        }

        if (!_useAdvancedMeshAPI) bufferUVArray.Dispose();
    }

}
