using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct VertexLayout
{
    public const ushort VERTEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.VERTICES;
    public const ushort INDEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.TRIANGLES;

    public Vector3 _position;
    public ushort _texCoordX, _texCoordY;

    // TODO: Refactor all over the system to use half (Float16 Bit), instead of ushort, in order to correct UV Mapping...
    // Example -> public half _texCoordX, _texCoordY;

    public static VertexAttributeDescriptor[] DefinedVertexLayout()
    {
        return new VertexAttributeDescriptor[]
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2, 0)
        };
    }

    public static void MergePositionLayoutWrapper(PositionLayoutWrapper positionLayout, ref VertexLayout vertexLayout)
        => vertexLayout._position = positionLayout._position;

    public static void MergeUVLayoutWrapper(UVLayoutWrapper UVLayout, ref VertexLayout vertexLayout)
    {
        vertexLayout._texCoordX = UVLayout._texCoordX;
        vertexLayout._texCoordY = UVLayout._textCoordY;
    }

    public struct PositionLayoutWrapper
    {
        public Vector3 _position;
    }

    public struct UVLayoutWrapper
    {
        public ushort _texCoordX, _textCoordY;

        private static readonly UVLayoutWrapper _default = new(0, 0);
        public static UVLayoutWrapper Default { get => _default; }

        public UVLayoutWrapper(ushort texCoordX, ushort textCoordY)
        {
            _texCoordX = texCoordX;
            _textCoordY = textCoordY;
        }
    }
}
