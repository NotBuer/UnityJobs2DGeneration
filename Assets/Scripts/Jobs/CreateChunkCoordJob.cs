using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkCoordJob : IJob
{
    public short _worldMiddleX;
    public short _worldMiddleY;
    public NativeArray<ChunkCoord> _chunksCoordsNativeArray;

    public CreateChunkCoordJob(short worldMiddleX, short worldMiddleY, NativeArray<ChunkCoord> chunksCoordsNativeArray)
    {
        _worldMiddleX = worldMiddleX;
        _worldMiddleY = worldMiddleY;
        _chunksCoordsNativeArray = chunksCoordsNativeArray;
    }

    public void Execute()
    {
        int iteration = 0;
        for (short y = (short)-_worldMiddleY; y < _worldMiddleY; y++)
        {
            for (short x = (short)-_worldMiddleX; x < _worldMiddleX; x++)
            {
                _chunksCoordsNativeArray[iteration] = new ChunkCoord(x * Chunk.X_SIZE, y * Chunk.Y_SIZE);
                iteration++;
            }
        }
    }
}
