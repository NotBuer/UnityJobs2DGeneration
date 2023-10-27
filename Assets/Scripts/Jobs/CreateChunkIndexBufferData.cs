using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public struct CreateChunkIndexBufferData : IJob
{
    public int _chunkCoordIndex;
    public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [WriteOnly] public NativeArray<int> _chunksIndicesNativeArray;
    public Mesh.MeshDataArray _chunkMeshDataArray;
    public bool _useAdvancedMeshAPI;

    public CreateChunkIndexBufferData(
        int chunkCoordIndex, 
        ChunkCoord chunkCoord, 
        NativeSlice<TileData> tileDataNativeSlice, 
        NativeArray<int> chunksIndicesNativeArray, 
        Mesh.MeshDataArray chunkMeshDataArray, 
        bool useAdvancedMeshAPI)
    {
        _chunkCoordIndex = chunkCoordIndex;
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _chunksIndicesNativeArray = chunksIndicesNativeArray;
        _chunkMeshDataArray = chunkMeshDataArray;
        _useAdvancedMeshAPI = useAdvancedMeshAPI;
    }

    public void Execute()
    {
        NativeArray<ushort> bufferTrianglesArray;

        if (_useAdvancedMeshAPI)
        {
            _chunkMeshDataArray[_chunkCoordIndex].SetIndexBufferParams(VertexLayout.INDEX_BUFFER_SIZE, IndexFormat.UInt16);
            bufferTrianglesArray = _chunkMeshDataArray[_chunkCoordIndex].GetIndexData<ushort>();
        }
        else bufferTrianglesArray = new(0, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        int triangleIndexFromStride = _chunkCoordIndex * VertexLayout.INDEX_BUFFER_SIZE;
        int triangleArrayIndex = 0;
        ushort vertexIndex = 0;

        for (byte y = 0; y < Chunk.Y_SIZE; y++)
        {
            for (byte x = 0; x < Chunk.X_SIZE; x++)
            {
                vertexIndex += Tile.VERTICES;

                if(_useAdvancedMeshAPI)
                {
                    bufferTrianglesArray[triangleArrayIndex++] = (ushort)(vertexIndex - 4);   // 0
                    bufferTrianglesArray[triangleArrayIndex++] = (ushort)(vertexIndex - 3);   // 1
                    bufferTrianglesArray[triangleArrayIndex++] = (ushort)(vertexIndex - 2);   // 2
                    bufferTrianglesArray[triangleArrayIndex++] = (ushort)(vertexIndex - 4);   // 0
                    bufferTrianglesArray[triangleArrayIndex++] = (ushort)(vertexIndex - 2);   // 2
                    bufferTrianglesArray[triangleArrayIndex++] = (ushort)(vertexIndex - 1);   // 3
                    continue;
                }

                _chunksIndicesNativeArray[triangleIndexFromStride++] = vertexIndex - 4;   // 0
                _chunksIndicesNativeArray[triangleIndexFromStride++] = vertexIndex - 3;   // 1
                _chunksIndicesNativeArray[triangleIndexFromStride++] = vertexIndex - 2;   // 2
                _chunksIndicesNativeArray[triangleIndexFromStride++] = vertexIndex - 4;   // 0
                _chunksIndicesNativeArray[triangleIndexFromStride++] = vertexIndex - 2;   // 2
                _chunksIndicesNativeArray[triangleIndexFromStride++] = vertexIndex - 1;   // 3
            }
        }

        if (!_useAdvancedMeshAPI) bufferTrianglesArray.Dispose();
    }
}
