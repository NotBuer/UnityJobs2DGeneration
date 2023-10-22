using UnityEngine;
using UnityEngine.Rendering;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct VertexLayout
{
    public const ushort VERTEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.VERTICES;
    public const ushort INDEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.TRIANGLES;

    public Vector3 _position;
    public ushort _texCoordX, _textCoordY;

    public static VertexAttributeDescriptor[] DefinedVertexLayout()
    {
        return new VertexAttributeDescriptor[]
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2, 0)
        };
    }
}
