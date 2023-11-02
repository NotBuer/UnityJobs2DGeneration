using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct VertexLayout
{
    public const ushort VERTEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.VERTICES;
    public const ushort INDEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.TRIANGLES;

    public half _positionX, _positionY;
    public half _texCoordX, _texCoordY;

    public static VertexAttributeDescriptor[] DefinedVertexLayout()
    {
        return new VertexAttributeDescriptor[]
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float16, 2),
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2)
        };
    }

    public static void MergePositionLayout(ref PositionLayout positionLayout, ref VertexLayout vertexLayout)
    {
        vertexLayout._positionX = positionLayout._positionX;
        vertexLayout._positionY = positionLayout._positionY;
    }

    public static void MergeUVLayout(ref UVLayout UVLayout, ref VertexLayout vertexLayout)
    {
        vertexLayout._texCoordX = UVLayout._texCoordX;
        vertexLayout._texCoordY = UVLayout._texCoordY;
    }

    public struct PositionLayout
    {
        public half _positionX;
        public half _positionY;

        public PositionLayout(half positionX, half positionY)
        {
            _positionX = positionX;
            _positionY = positionY;
        }
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
