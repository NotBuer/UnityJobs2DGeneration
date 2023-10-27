using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static TextureAtlas;

[BurstCompile]
public struct CreateChunkVertexBufferDataJob : IJob
{
    public int _chunkCoordIndex;
    public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [WriteOnly] public NativeArray<Vector3> _chunksVerticesNativeArray;
    [WriteOnly] public NativeArray<Vector2> _chunksUVSNativeArray;
    public Mesh.MeshDataArray _chunkMeshDataArray;
    NativeArray<VertexAttributeDescriptor> _bufferLayout;
    public bool _useAdvancedMeshAPI;

    public CreateChunkVertexBufferDataJob(
        int chunkCoordIndex, 
        ChunkCoord chunkCoord, 
        NativeSlice<TileData> tileDataNativeSlice, 
        NativeArray<Vector3> chunksVerticesNativeArray, 
        NativeArray<Vector2> chunksUVSNativeArray, 
        Mesh.MeshDataArray chunkMeshDataArray, 
        NativeArray<VertexAttributeDescriptor> bufferLayout, 
        bool useAdvancedMeshAPI)
    {
        _chunkCoordIndex = chunkCoordIndex;
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunksVerticesNativeArray = chunksVerticesNativeArray;
        _chunksUVSNativeArray = chunksUVSNativeArray;
        _chunkMeshDataArray = chunkMeshDataArray;
        _bufferLayout = bufferLayout;
        _useAdvancedMeshAPI = useAdvancedMeshAPI;
    }

    public void Execute()
    {
        // TIP: Is possible to pass the '_chunkcoord' coordinates incrementing the vertex position,
        // converting all chunk tiles from Local Position to World Space Position.

        NativeArray<VertexLayout> bufferVertexArray;
        if (_useAdvancedMeshAPI)
        {
            _chunkMeshDataArray[_chunkCoordIndex].SetVertexBufferParams(VertexLayout.VERTEX_BUFFER_SIZE, _bufferLayout);
            bufferVertexArray = _chunkMeshDataArray[_chunkCoordIndex].GetVertexData<VertexLayout>();
        }
        else bufferVertexArray = new(0, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        int indexOffsetByStride = _chunkCoordIndex * VertexLayout.VERTEX_BUFFER_SIZE;
        int sliceIndex = 0;
        int vertexArrayIndex = 0;
        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                TileData tileData = _tileDataNativeSlice[sliceIndex];

                var LeftBottomUV = GetUVTextureForTile(tileData, TexCoord.LEFT_BOTTOM);
                var LeftTopUV = GetUVTextureForTile(tileData, TexCoord.LEFT_TOP);
                var RigthTopUV = GetUVTextureForTile(tileData, TexCoord.RIGHT_TOP);
                var RigthBottomUV = GetUVTextureForTile(tileData, TexCoord.RIGHT_BOTTOM);

                var LeftBottomPosition = new VertexLayout.PositionLayout(new Vector3(x, y));
                var LeftTopPosition = new VertexLayout.PositionLayout(new Vector3(x, 1 + y));
                var RigthTopPosition = new VertexLayout.PositionLayout(new Vector3(x + 1, y + 1));
                var RigthBottomPosition = new VertexLayout.PositionLayout(new Vector3(x + 1, y));

                if (_useAdvancedMeshAPI)
                {
                    VertexLayout LeftBottom = bufferVertexArray[vertexArrayIndex];
                    VertexLayout.MergePositionLayout(ref LeftBottomPosition, ref LeftBottom);
                    VertexLayout.MergeUVLayout(ref LeftBottomUV, ref LeftBottom);
                    bufferVertexArray[vertexArrayIndex] = LeftBottom;

                    VertexLayout LeftTop = bufferVertexArray[vertexArrayIndex + 1];
                    VertexLayout.MergePositionLayout(ref LeftTopPosition, ref LeftTop);
                    VertexLayout.MergeUVLayout(ref LeftTopUV, ref LeftTop);
                    bufferVertexArray[vertexArrayIndex + 1] = LeftTop;

                    VertexLayout RigthTop = bufferVertexArray[vertexArrayIndex + 2];
                    VertexLayout.MergePositionLayout(ref RigthTopPosition, ref RigthTop);
                    VertexLayout.MergeUVLayout(ref RigthTopUV, ref RigthTop);
                    bufferVertexArray[vertexArrayIndex + 2] = RigthTop;

                    VertexLayout RigthBottom = bufferVertexArray[vertexArrayIndex + 3];
                    VertexLayout.MergePositionLayout(ref RigthBottomPosition, ref RigthBottom);
                    VertexLayout.MergeUVLayout(ref RigthBottomUV, ref RigthBottom);
                    bufferVertexArray[vertexArrayIndex + 3] = RigthBottom;

                    vertexArrayIndex += Tile.VERTICES;
                    sliceIndex++;
                    continue;
                }

                _chunksVerticesNativeArray[indexOffsetByStride] = new Vector3(x, y);          //(0, 0)
                _chunksVerticesNativeArray[indexOffsetByStride+1] = new Vector3(x, 1 + y);      //(0, 1)
                _chunksVerticesNativeArray[indexOffsetByStride+2] = new Vector3(x + 1, y + 1);  //(1, 1)
                _chunksVerticesNativeArray[indexOffsetByStride+3] = new Vector3(x + 1, y);      //(1, 0)

                // TODO: Aplicar pattern VertexLayoutWrapper...
                _chunksUVSNativeArray[indexOffsetByStride] = new Vector2(0, 0);   //(0, 0)
                _chunksUVSNativeArray[indexOffsetByStride+1] = new Vector2(0, 1);   //(0, 1)
                _chunksUVSNativeArray[indexOffsetByStride+2] = new Vector2(1, 1);   //(1, 1)
                _chunksUVSNativeArray[indexOffsetByStride+3] = new Vector2(1, 0);   //(1, 0)

                indexOffsetByStride += Tile.VERTICES;
            }
        }

        if (!_useAdvancedMeshAPI) bufferVertexArray.Dispose();
    }
}
