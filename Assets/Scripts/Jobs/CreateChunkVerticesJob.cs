using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkVerticesJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _stride;
    [WriteOnly] public NativeArray<Vector2Int> _worldChunksVerticesNativeArray;

    public CreateChunkVerticesJob(
        ChunkCoord chunkCoord, 
        NativeSlice<TileData> tileDataNativeSlice, 
        int stride, 
        NativeArray<Vector2Int> worldChunksVerticesNativeArray)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _stride = stride;
        _worldChunksVerticesNativeArray = worldChunksVerticesNativeArray;
    }

    public void Execute()
    {
        int verticesBuffer = (Chunk.TOTAL_SIZE * Tile.VERTICES);
        int start = _stride * verticesBuffer;
        for (int i = start; i < verticesBuffer; i += Tile.VERTICES)
        {
            _worldChunksVerticesNativeArray[i + 0] = new Vector2Int(0, 0);
            _worldChunksVerticesNativeArray[i + 1] = new Vector2Int(0, 1);
            _worldChunksVerticesNativeArray[i + 2] = new Vector2Int(1, 1);
            _worldChunksVerticesNativeArray[i + 3] = new Vector2Int(1, 0);
        }
    }
}
