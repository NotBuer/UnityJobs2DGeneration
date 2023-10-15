using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkCoordJob : IJob
{
    public short worldMiddleX;
    public short worldMiddleY;
    public NativeArray<ChunkCoord> chunksCoordsNativeArray;

    public void Execute()
    {
        int iteration = 0;
        for (short y = (short)-worldMiddleY; y < worldMiddleY; y++)
        {
            for (short x = (short)-worldMiddleX; x < worldMiddleX; x++)
            {
                chunksCoordsNativeArray[iteration] = new ChunkCoord(x * Chunk.X_SIZE, y * Chunk.Y_SIZE);
                iteration++;
            }
        }
    }
}
