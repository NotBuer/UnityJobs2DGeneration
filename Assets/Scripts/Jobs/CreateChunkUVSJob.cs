using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static TextureAtlas;

[BurstCompile]
public struct CreateChunkUVSJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _chunkCoordIndex;
    [WriteOnly] public NativeArray<Vector2> _chunksUVSNativeArray;
    public Mesh.MeshDataArray _chunkMeshDataArray;
    public bool _useAdvancedMeshAPI;

    public CreateChunkUVSJob(
        ChunkCoord chunkCoord,
        NativeSlice<TileData> tileDataNativeSlice,
        int chunkCoordIndex,
        NativeArray<Vector2> chunksUVSNativeArray,
        Mesh.MeshDataArray chunkMeshDataArray,
        bool useAdvancedMeshAPI)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunkCoordIndex = chunkCoordIndex;
        _chunksUVSNativeArray = chunksUVSNativeArray;
        _chunkMeshDataArray = chunkMeshDataArray;
        _useAdvancedMeshAPI = useAdvancedMeshAPI;
    }

    public void Execute()
    {
        NativeArray<VertexLayout> bufferUVArray;

        if (_useAdvancedMeshAPI)
        {
            bufferUVArray = _chunkMeshDataArray[_chunkCoordIndex].GetVertexData<VertexLayout>();
        }
        else bufferUVArray = new(0, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        int uvIndexFromStride = _chunkCoordIndex * VertexLayout.VERTEX_BUFFER_SIZE;
        int uvIndexArraySlice = 0;
        int uvArrayIndex = 0;

        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                TileData tileData = _tileDataNativeSlice[uvIndexArraySlice];

                var LeftBottomUV = GetUVTextureForTile(tileData, TexCoord.LEFT_BOTTOM);
                var LeftTopUV = GetUVTextureForTile(tileData, TexCoord.LEFT_TOP);
                var RigthTopUV = GetUVTextureForTile(tileData, TexCoord.RIGHT_TOP);
                var RigthBottomUV = GetUVTextureForTile(tileData, TexCoord.RIGHT_BOTTOM);

                if (_useAdvancedMeshAPI)
                {
                    VertexLayout LeftBottom = bufferUVArray[uvArrayIndex];
                    VertexLayout.MergeUVLayout(ref LeftBottomUV, ref LeftBottom);
                    bufferUVArray[uvArrayIndex] = LeftBottom;

                    VertexLayout LeftTop = bufferUVArray[uvArrayIndex + 1];
                    VertexLayout.MergeUVLayout(ref LeftTopUV, ref LeftTop);
                    bufferUVArray[uvArrayIndex + 1] = LeftTop;

                    VertexLayout RigthTop = bufferUVArray[uvArrayIndex + 2];
                    VertexLayout.MergeUVLayout(ref RigthTopUV, ref RigthTop);
                    bufferUVArray[uvArrayIndex + 2] = RigthTop;

                    VertexLayout RigthBottom = bufferUVArray[uvArrayIndex + 3];
                    VertexLayout.MergeUVLayout(ref RigthBottomUV, ref RigthBottom);
                    bufferUVArray[uvArrayIndex + 3] = RigthBottom;

                    uvArrayIndex += Tile.UVS;
                    uvIndexArraySlice++;
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
