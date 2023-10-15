using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateChunkMeshDataJob : IJob
{
    //[ReadOnly] public NativeArray<ChunkCoord> chunkCoordNativeArray;
    //[ReadOnly] public NativeArray<TileData> tileDataNativeArray;
    //[WriteOnly] public NativeArray<Vector2Int> worldChunksVerticesNativeArray;
    //[WriteOnly] public NativeArray<int> worldChunksTrianglesNativeArray;
    //[WriteOnly] public NativeArray<Vector2> worldChunksUVSNativeArray;

    //public void Execute()
    //{
    //    JobHandle dependencyHandle = new();

    //    for (int i = 0; i < chunkCoordNativeArray.Length; i++)
    //    {
    //        NativeSlice<TileData> tileDataNativeSlice = new(tileDataNativeArray, (i * Chunk.TOTAL_SIZE) + i, Chunk.TOTAL_SIZE);

    //        CreateChunkVerticesJob createChunkVerticesJob = new CreateChunkVerticesJob()
    //        {
    //            chunkCoord = chunkCoordNativeArray[i],
    //            tileDataNativeSlice = tileDataNativeSlice,
    //            stride = i,
    //            worldChunksVerticesNativeArray = worldChunksVerticesNativeArray,
    //        };
    //        JobHandle jobHandleVertices = createChunkVerticesJob.Schedule(dependencyHandle);

    //        CreateChunkTrianglesJob createChunkTrianglesJob = new CreateChunkTrianglesJob()
    //        {
    //            chunkCoord = chunkCoordNativeArray[i],
    //            tileDataNativeSlice = tileDataNativeSlice,
    //            stride = i,
    //            worldChunksTrianglesArray = worldChunksTrianglesNativeArray
    //        };
    //        JobHandle jobHandleTriangles = createChunkTrianglesJob.Schedule(dependencyHandle);

    //        dependencyHandle = JobHandle.CombineDependencies(jobHandleVertices, jobHandleTriangles);
    //    }

    //    dependencyHandle.Complete();
    //}
}
