using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct CreateChunkUVSJob : IJob
{
    [ReadOnly] public ChunkCoord _chunkCoord;
    [ReadOnly] public NativeSlice<TileData> _tileDataNativeSlice;
    [ReadOnly] public int _stride;
    [WriteOnly] public NativeArray<Vector2> _worldChunksUVSNativeArray;

    public CreateChunkUVSJob(
        ChunkCoord chunkCoord,
        NativeSlice<TileData> tileDataNativeSlice,
        int stride,
        NativeArray<Vector2> worldChunksUVSNativeArray)
    {
        _chunkCoord = chunkCoord;
        _tileDataNativeSlice = tileDataNativeSlice;
        _stride = stride;
        _worldChunksUVSNativeArray = worldChunksUVSNativeArray;
    }

    public void Execute()
    {
        int verticesBuffer = (Chunk.TOTAL_SIZE * Tile.UVS);
        int start = _stride * verticesBuffer;
        for (int i = start; i < verticesBuffer; i += Tile.UVS)
        {
            _worldChunksUVSNativeArray[i + 0] = new Vector2(0, 0);
            _worldChunksUVSNativeArray[i + 1] = new Vector2(0, 1);
            _worldChunksUVSNativeArray[i + 2] = new Vector2(1, 1);
            _worldChunksUVSNativeArray[i + 3] = new Vector2(1, 0);
        }
    }
}
