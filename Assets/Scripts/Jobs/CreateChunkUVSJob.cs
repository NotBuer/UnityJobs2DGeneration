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
            bufferUVArray = _chunkMeshDataArray[_chunkCoordIndex].GetVertexData<VertexLayout>();
        }
        else bufferUVArray = new NativeArray<VertexLayout>(0, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        int uvIndexFromStride = _chunkCoordIndex * VertexLayout.VERTEX_BUFFER_SIZE;
        int uvArrayIndex = 0;

        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                TileData tileData = _tileDataNativeSlice[uvIndexFromStride];

                var LeftBottomUV = SpriteAtlasUtils.GetUVTextureForTile(tileData);
                var LeftTopUV = SpriteAtlasUtils.GetUVTextureForTile(tileData);
                var RigthTopUV = SpriteAtlasUtils.GetUVTextureForTile(tileData);
                var RigthBottomUV = SpriteAtlasUtils.GetUVTextureForTile(tileData);

                if (_useAdvancedMeshAPI)
                {
                    VertexLayout LeftBottom = bufferUVArray[uvArrayIndex];
                    VertexLayout.MergeUVLayoutWrapper(LeftBottomUV, ref LeftBottom);
                    bufferUVArray[uvArrayIndex] = LeftBottom;

                    //VertexLayout LeftBottom = bufferUVArray[uvArrayIndex];
                    //LeftBottom._texCoordX = 0;
                    //LeftBottom._texCoordY = 0;
                    //bufferUVArray[uvArrayIndex] = LeftBottom;

                    VertexLayout LeftTop = bufferUVArray[uvArrayIndex + 1];
                    VertexLayout.MergeUVLayoutWrapper(LeftTopUV, ref LeftTop);
                    bufferUVArray[uvArrayIndex + 1] = LeftTop;
                    //VertexLayout LeftTop = bufferUVArray[uvArrayIndex + 1];
                    //LeftTop._texCoordX = 0;
                    //LeftTop._texCoordY = 1;
                    //bufferUVArray[uvArrayIndex + 1] = LeftTop;

                    VertexLayout RigthTop = bufferUVArray[uvArrayIndex + 2];
                    VertexLayout.MergeUVLayoutWrapper(RigthTopUV, ref RigthTop);
                    bufferUVArray[uvArrayIndex + 2] = RigthTop;
                    //VertexLayout RigthTop = bufferUVArray[uvArrayIndex + 2];
                    //RigthTop._texCoordX = 1;
                    //RigthTop._texCoordY = 1;
                    //bufferUVArray[uvArrayIndex + 2] = RigthTop;

                    VertexLayout RigthBottom = bufferUVArray[uvArrayIndex + 3];
                    VertexLayout.MergeUVLayoutWrapper(RigthBottomUV, ref RigthBottom);
                    bufferUVArray[uvArrayIndex + 3] = RigthBottom;
                    //VertexLayout RigthBottom = bufferUVArray[uvArrayIndex + 3];
                    //RigthBottom._texCoordX = 1;
                    //RigthBottom._texCoordY = 0;
                    //bufferUVArray[uvArrayIndex + 3] = RigthBottom;

                    uvArrayIndex += Tile.UVS;
                    continue;
                }

                // TODO: Aplicar pattern VertexLayoutWrapper...
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(0, 0);   //(0, 0)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(0, 1);   //(0, 1)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(1, 1);   //(1, 1)
                _chunksUVSNativeArray[uvIndexFromStride++] = new Vector2(1, 0);   //(1, 0)
            }
        }

        if (!_useAdvancedMeshAPI) bufferUVArray.Dispose();
    }

}
