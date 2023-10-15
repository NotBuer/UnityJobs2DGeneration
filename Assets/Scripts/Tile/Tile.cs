public static class Tile
{
    public const byte VERTICES = 4;
    public const byte UVS = 4;
    public const byte TRIANGLES = 6;

    //public static void CreateVertices(ref NativeArray<Vector3> vertices)
    //{
    //    vertices[0] = new Vector3(0, 0);
    //    vertices[1] = new Vector3(0, 1);
    //    vertices[2] = new Vector3(1, 1);
    //    vertices[3] = new Vector3(1, 0);
    //}

    //public static void CreateTriangles(ref NativeArray<int> triangles)
    //{
    //    triangles[0] = 0;
    //    triangles[1] = 1;
    //    triangles[2] = 2;
    //    triangles[3] = 0;
    //    triangles[4] = 2;
    //    triangles[5] = 3;
    //}

    //public static void CreateUvs(ref NativeArray<Vector2> uvs)
    //{
    //    uvs[0] = new Vector2(0, 0);
    //    uvs[1] = new Vector2(0, 1);
    //    uvs[2] = new Vector2(1, 1);
    //    uvs[3] = new Vector2(1, 0);
    //}

    public enum TileType : byte
    {
        Air = 0,
        Dirt = 1,
        Grass = 2,
        Stone = 3,
    }
}
