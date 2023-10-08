using Unity.Collections;
using UnityEngine;

public struct TileMeshData
{
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector2> uvs;
    public NativeArray<int> triangles;

    public TileMeshData(Allocator allocator)
    {
        vertices = new NativeArray<Vector3>(Tile.VERTICES, allocator);
        uvs = new NativeArray<Vector2>(Tile.UVS, allocator);
        triangles = new NativeArray<int>(Tile.TRIANGLES, allocator);
    }
}
