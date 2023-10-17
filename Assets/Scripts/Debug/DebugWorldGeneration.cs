using System.Collections;
using Unity.Collections;
using UnityEngine;

public static class DebugWorldGeneration
{
    public static IEnumerator DebugCreateChunkCoord(NativeArray<ChunkCoord> ChunkCoordNativeArray)
    {
        foreach (var chunkCoord in ChunkCoordNativeArray)
        {
            Debug.Log($"ChunkCoord - X: {chunkCoord.XCoord} / Y: {chunkCoord.YCoord}");
            yield return new WaitForEndOfFrame();
        }
    }

    public static IEnumerator DebugCreateTileCoord(NativeArray<TileCoord> TileCoordNativeArray)
    {
        foreach (var tileCoord in TileCoordNativeArray)
        {
            Debug.Log($"Tile: XCoord {tileCoord.XCoord} / YCoord {tileCoord.YCoord}");
            yield return new WaitForEndOfFrame();
        }
    }

    public static IEnumerator DebugCreateTileData(NativeArray<TileData> TileDataNativeArray)
    {
        foreach (var tileData in TileDataNativeArray)
        {
            Debug.Log($"Tile: {tileData.Type}");
            yield return new WaitForEndOfFrame();
        }
    }

    public static IEnumerator DebugWorldChunksVerticesData(NativeArray<Vector3Int> WorldChunksVerticesNativeArray)
    {
        foreach (var vertex in WorldChunksVerticesNativeArray)
        {
            Debug.Log($"Vertice: {vertex}");
            yield return new WaitForEndOfFrame();
        }
    }

    public static IEnumerator DebugWorldChunksTrianglesData(NativeArray<int> WorldChunksTrianglesNativeArray)
    {
        foreach (var triangle in WorldChunksTrianglesNativeArray)
        {
            Debug.Log($"Triangle: {triangle}");
            yield return new WaitForEndOfFrame();
        }
    }

    public static IEnumerator DebugWorldChunksUVSData(NativeArray<Vector2> WorldChunksUVSNativeArray)
    {
        foreach (var uv in WorldChunksUVSNativeArray)
        {
            Debug.Log($"UV: {uv}");
            yield return new WaitForEndOfFrame();
        }
    }
}
