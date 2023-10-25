using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct VertexLayout
{
    public const ushort VERTEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.VERTICES;
    public const ushort INDEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.TRIANGLES;

    public Vector3 _position;
    public half _texCoordX, _texCoordY;

    public static VertexAttributeDescriptor[] DefinedVertexLayout()
    {
        return new VertexAttributeDescriptor[]
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2, 0)
        };
    }

    public static void MergePositionLayoutWrapper(ref PositionLayout positionLayout, ref VertexLayout vertexLayout)
        => vertexLayout._position = positionLayout._position;

    public static void MergeUVLayoutWrapper(ref UVLayout UVLayout, ref VertexLayout vertexLayout)
    {
        vertexLayout._texCoordX = UVLayout._texCoordX;
        vertexLayout._texCoordY = UVLayout._texCoordY;
    }

    public struct PositionLayout
    {
        public Vector3 _position;
    }

    public struct UVLayout
    {
        public half _texCoordX, _texCoordY;

        private static readonly UVLayout _default = new(half.zero, half.zero);
        public static UVLayout Default { get => _default; }

        public UVLayout(half texCoordX, half texCoordY)
        {
            _texCoordX = texCoordX;
            _texCoordY = texCoordY;
        }
    }
}
