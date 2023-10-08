using Unity.Collections;
using UnityEngine;

public struct ChunkMeshData
{
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector2> uvs;
    public NativeArray<int> triangles;

    public ChunkMeshData(Allocator allocator)
    {
        vertices = new NativeArray<Vector3>(Tile.VERTICES * Chunk.TOTAL_SIZE, allocator);
        uvs = new NativeArray<Vector2>(Tile.UVS * Chunk.TOTAL_SIZE, allocator);
        triangles = new NativeArray<int>(Tile.TRIANGLES * Chunk.TOTAL_SIZE, allocator);
    }
}
