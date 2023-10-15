using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct CreateChunkTrianglesJob : IJob
{
    [ReadOnly] public ChunkCoord chunkCoord;
    [ReadOnly] public NativeSlice<TileData> tileDataNativeSlice;
    [ReadOnly] public int stride;
    [WriteOnly] public NativeArray<int> worldChunksTrianglesArray;

    public void Execute()
    {
        int verticesBuffer = (Chunk.TOTAL_SIZE * Tile.TRIANGLES);
        int start = stride * verticesBuffer;
        for (int i = start; i < verticesBuffer; i += Tile.TRIANGLES)
        {
            worldChunksTrianglesArray[i + 0] = 0;
            worldChunksTrianglesArray[i + 1] = 1;
            worldChunksTrianglesArray[i + 2] = 2;
            worldChunksTrianglesArray[i + 3] = 0;
            worldChunksTrianglesArray[i + 4] = 2;
            worldChunksTrianglesArray[i + 5] = 3;
        }
    }
}
