using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkVerticesJob : IJob
{
    [ReadOnly] public ChunkCoord chunkCoord;
    [ReadOnly] public NativeSlice<TileData> tileDataNativeSlice;
    [ReadOnly] public int stride;
    [WriteOnly] public NativeArray<Vector2Int> worldChunksVerticesNativeArray;

    public void Execute()
    {
        int verticesBuffer = (Chunk.TOTAL_SIZE * Tile.VERTICES);
        int start = stride * verticesBuffer;
        for (int i = start; i < verticesBuffer; i += Tile.VERTICES)
        {
            worldChunksVerticesNativeArray[i + 0] = new Vector2Int(0, 0);
            worldChunksVerticesNativeArray[i + 1] = new Vector2Int(0, 1);
            worldChunksVerticesNativeArray[i + 2] = new Vector2Int(1, 1);
            worldChunksVerticesNativeArray[i + 3] = new Vector2Int(1, 0);
        }
    }
}
