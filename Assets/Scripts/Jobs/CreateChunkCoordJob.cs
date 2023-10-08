using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct CreateChunkCoordJob : IJob
{
    public short worldMiddleX;
    public short worldMiddleY;
    public NativeArray<ChunkCoord> chunksCoords;

    public void Execute()
    {
        int iteration = 0;
        for (short y = (short)-worldMiddleY; y < worldMiddleY; y++)
        {
            for (short x = (short)-worldMiddleX; x < worldMiddleX; x++)
            {
                chunksCoords[iteration] = new ChunkCoord(x, y);
                iteration++;
            }
        }
    }
}
