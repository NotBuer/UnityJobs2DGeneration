using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class World : MonoBehaviour
{
    // Size in chunks.
    private const short WORLD_X_SIZE = 1024;
    private const short WORLD_Y_SIZE = 1024;

    public const short CHUNK_VERTEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.VERTICES;
    public const short CHUNK_TRIANGLE_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.TRIANGLES;
    public const short CHUNK_UV_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.UVS;

    [Header("Debugging")]

    [Range(0, WORLD_X_SIZE)]
    [SerializeField]
    private short XSizeInChunks;

    [Range(0, WORLD_Y_SIZE)]
    [SerializeField]
    private short YSizeInChunks;

    [SerializeField] private bool DebugLogChunkCoordGeneration = false;
    [SerializeField] private bool DebugLogTileCoordGeneration = false;
    [SerializeField] private bool DebugLogTileDataGeneration = false;
    [SerializeField] private bool DebugLogWorldChunksVerticesData = false;
    [SerializeField] private bool DebugLogWorldChunksTrianglesData = false;
    [SerializeField] private bool DebugLogWorldChunksUVSData = false;
    [SerializeField] private bool PauseChunkMeshGeneration = false;

    private short _worldMiddleX = 0;
    private short _worldMiddleY = 0;

    public NativeArray<ChunkCoord> ChunkCoordNativeArray { get; set; }
    public NativeArray<TileCoord> TileCoordNativeArray { get; set; }
    public NativeArray<TileData> TileDataNativeArray { get; set; }
    public NativeArray<Vector3Int> ChunksVerticesNativeArray { get; set; }
    public NativeArray<int> ChunksTrianglesNativeArray { get; set; }
    public NativeArray<Vector2> ChunksUVSNativeArray { get; set; }

    private JobHandle _jobHandleCreateChunkCoord; 
    private JobHandle _jobHandleCreateTileCoord;
    private JobHandle _jobHandleCreateTileData;
    private JobHandle _jobHandleRawWorldDataGenerated;
    private JobHandle _jobHandleCreateWorldChunksMeshData;

    private bool _createChunkCoordsOnce = false;
    private bool _createTileCoordsOnce = false;
    private bool _createTileDataOnce = false;
    private bool _rawWorldDataGenerated = false;
    private bool _createWorldChunksMeshDataOnce = false;

    private void Awake() 
    {
        ChunkCoordNativeArray = new(XSizeInChunks * YSizeInChunks, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        TileCoordNativeArray = new((Chunk.TOTAL_SIZE) * ChunkCoordNativeArray.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        TileDataNativeArray = new((Chunk.TOTAL_SIZE) * ChunkCoordNativeArray.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        ChunksVerticesNativeArray = new((XSizeInChunks * YSizeInChunks) * CHUNK_VERTEX_BUFFER_SIZE, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        ChunksTrianglesNativeArray = new((XSizeInChunks * YSizeInChunks) * CHUNK_TRIANGLE_BUFFER_SIZE, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        ChunksUVSNativeArray = new((XSizeInChunks * YSizeInChunks) * CHUNK_UV_BUFFER_SIZE, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        _worldMiddleX = (short)(XSizeInChunks / 2);
        _worldMiddleY = (short)(YSizeInChunks / 2);
    }

    void Start()
    {
        CreateChunkCoordJob createChunksJob = new(_worldMiddleX, _worldMiddleY, ChunkCoordNativeArray);
        _jobHandleCreateChunkCoord = createChunksJob.Schedule();
        Debug.Log("WORLD - Scheduling generation of ChunkCoord...");

        CreateTileCoordJob createTileCoordJob = new(ChunkCoordNativeArray, TileCoordNativeArray);
        _jobHandleCreateTileCoord = createTileCoordJob.Schedule(_jobHandleCreateChunkCoord);
        Debug.Log("WORLD - Scheduling generation of TileCoord...");

        CreateTileDataJob createTileDataJob = new(ChunkCoordNativeArray, TileDataNativeArray);
        _jobHandleCreateTileData = createTileDataJob.Schedule(_jobHandleCreateChunkCoord);
        Debug.Log("WORLD - Scheduling generation of TileData...");

        _jobHandleRawWorldDataGenerated = JobHandle.CombineDependencies(_jobHandleCreateChunkCoord, _jobHandleCreateTileCoord, _jobHandleCreateTileData);

        //Mesh mesh = new Mesh();

        //Vector3[] verticesArray = new Vector3[CHUNK_VERTEX_BUFFER_SIZE];
        //int[] trianglesArray = new int[CHUNK_TRIANGLE_BUFFER_SIZE];
        //Vector2[] uvsArray = new Vector2[CHUNK_UV_BUFFER_SIZE];

        //int vertexIndex = 0;
        //int triangleIndex = 0;
        //int uvIndex = 0;

        //for (int y = 0; y < Chunk.Y_SIZE; y++)
        //{
        //    for (int x = 0; x < Chunk.X_SIZE; x++)
        //    {
        //        int xPos = x;
        //        int yPos = y;

        //        verticesArray[vertexIndex++] = new Vector3Int(xPos, yPos);
        //        verticesArray[vertexIndex++] = new Vector3Int(xPos, yPos + 1);
        //        verticesArray[vertexIndex++] = new Vector3Int(xPos + 1, yPos + 1);
        //        verticesArray[vertexIndex++] = new Vector3Int(xPos + 1, yPos);

        //        trianglesArray[triangleIndex++] = vertexIndex - 4; // 0
        //        trianglesArray[triangleIndex++] = vertexIndex - 3; // 1
        //        trianglesArray[triangleIndex++] = vertexIndex - 2; // 2
        //        trianglesArray[triangleIndex++] = vertexIndex - 4; // 0
        //        trianglesArray[triangleIndex++] = vertexIndex - 2; // 2
        //        trianglesArray[triangleIndex++] = vertexIndex - 1; // 3

        //        uvsArray[uvIndex++] = new Vector2(0, 0);
        //        uvsArray[uvIndex++] = new Vector2(0, 1);
        //        uvsArray[uvIndex++] = new Vector2(1, 1);
        //        uvsArray[uvIndex++] = new Vector2(1, 0);
        //    }
        //}

        //mesh.vertices = verticesArray;
        //mesh.triangles = trianglesArray;
        //mesh.uv = uvsArray;
        //mesh.RecalculateBounds();

        //gameObject.AddComponent<MeshFilter>().mesh = mesh;
        //gameObject.AddComponent<MeshRenderer>();
    }

    private void LateUpdate()
    {
        if (_jobHandleCreateChunkCoord.IsCompleted && !_createChunkCoordsOnce)
        {
            _createChunkCoordsOnce = true;
            _jobHandleCreateChunkCoord.Complete();

            Debug.Log("WORLD - ChunkCoords generated successfully!");
            Debug.Log($"WORLD - ChunkCoords array size: {ChunkCoordNativeArray.Length}");
            if (DebugLogChunkCoordGeneration) StartCoroutine(DebugWorldGeneration.DebugCreateChunkCoord(ChunkCoordNativeArray));
        }

        if (_jobHandleCreateTileCoord.IsCompleted && !_createTileCoordsOnce)
        {
            _createTileCoordsOnce = true;
            _jobHandleCreateTileCoord.Complete();

            Debug.Log("WORLD - TileCoord generated successfully!");
            Debug.Log($"WORLD - TileCoord array size: {TileCoordNativeArray.Length}");
            if (DebugLogTileCoordGeneration) StartCoroutine(DebugWorldGeneration.DebugCreateTileCoord(TileCoordNativeArray));
        }

        if (_jobHandleCreateTileData.IsCompleted && !_createTileDataOnce)
        {
            _createTileDataOnce = true;
            _jobHandleCreateTileData.Complete();

            Debug.Log("WORLD - TileData generated successfully!");
            Debug.Log($"WORLD - TileData array size: {TileDataNativeArray.Length}");
            if (DebugLogTileDataGeneration) StartCoroutine(DebugWorldGeneration.DebugCreateTileData(TileDataNativeArray));
        }

        // Wait all world raw data be built, in order to start generating the chunks mesh data.
        if (_jobHandleRawWorldDataGenerated.IsCompleted && !_rawWorldDataGenerated)
        {
            _rawWorldDataGenerated = true;
            _jobHandleRawWorldDataGenerated.Complete();
            GenerateChunkMeshData();
        }

        if (_jobHandleCreateWorldChunksMeshData.IsCompleted && !_createWorldChunksMeshDataOnce)
        {
            _createWorldChunksMeshDataOnce = true;
            _jobHandleCreateWorldChunksMeshData.Complete();
            StartCoroutine(YieldedChunkMeshGeneration());

            Debug.Log("WORLD - World Chunks Mesh Data generated successfully!");
            Debug.Log($"WORLD - Vertices array size: {ChunksVerticesNativeArray.Length}");
            Debug.Log($"WORLD - Triangles array size: {ChunksTrianglesNativeArray.Length}");
            Debug.Log($"WORLD - UVS array size: {ChunksUVSNativeArray.Length}");
            if (DebugLogWorldChunksVerticesData) StartCoroutine(DebugWorldGeneration.DebugWorldChunksVerticesData(ChunksVerticesNativeArray));
            if (DebugLogWorldChunksTrianglesData) StartCoroutine(DebugWorldGeneration.DebugWorldChunksTrianglesData(ChunksTrianglesNativeArray));
            if (DebugLogWorldChunksUVSData) StartCoroutine(DebugWorldGeneration.DebugWorldChunksUVSData(ChunksUVSNativeArray));
        }
    }

    private void GenerateChunkMeshData()
    {
        Debug.Log("WORLD - Scheduling generation of World Chunks Mesh Data...");
        _jobHandleCreateWorldChunksMeshData = _jobHandleRawWorldDataGenerated;

        NativeArray<JobHandle> jobDependenciesHandleNativeArray = new NativeArray<JobHandle>(4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        for (int index = 0; index < ChunkCoordNativeArray.Length; index++) 
        {
            NativeSlice<TileData> tileDataNativeSlice = new(TileDataNativeArray, (index * Chunk.TOTAL_SIZE), Chunk.TOTAL_SIZE);

            CreateChunkVerticesJob createChunkVerticesJob = new(ChunkCoordNativeArray[index], tileDataNativeSlice, index, ChunksVerticesNativeArray);
            JobHandle jobHandleCreateVertices = createChunkVerticesJob.Schedule(_jobHandleCreateWorldChunksMeshData);

            CreateChunkTrianglesJob createChunkTrianglesJob = new(ChunkCoordNativeArray[index], tileDataNativeSlice, index, ChunksTrianglesNativeArray);
            JobHandle jobHandleCreateTriangles = createChunkTrianglesJob.Schedule(_jobHandleCreateWorldChunksMeshData);

            CreateChunkUVSJob createChunkUVSJob = new(ChunkCoordNativeArray[index], tileDataNativeSlice, index, ChunksUVSNativeArray);
            JobHandle jobHandleCreateUVS = createChunkUVSJob.Schedule(_jobHandleCreateWorldChunksMeshData);

            jobDependenciesHandleNativeArray[0] = _jobHandleCreateWorldChunksMeshData;
            jobDependenciesHandleNativeArray[1] = jobHandleCreateVertices;
            jobDependenciesHandleNativeArray[2] = jobHandleCreateTriangles;
            jobDependenciesHandleNativeArray[3] = jobHandleCreateUVS;
            _jobHandleCreateWorldChunksMeshData = JobHandle.CombineDependencies(jobDependenciesHandleNativeArray);
        }

        _jobHandleCreateWorldChunksMeshData.Complete();
        jobDependenciesHandleNativeArray.Dispose();
    }

    private IEnumerator YieldedChunkMeshGeneration()
    {
        //ChunkCoordNativeArray.Length
        for (int i = 0; i <= 1; i++)
        {
            yield return new WaitWhile(() => PauseChunkMeshGeneration);

            NativeSlice<Vector3Int> chunksVerticesArraySlice = ChunksVerticesNativeArray.Slice(
                i * CHUNK_VERTEX_BUFFER_SIZE, 
                CHUNK_VERTEX_BUFFER_SIZE);

            NativeSlice<int> chunksTriangleArraySlice = ChunksTrianglesNativeArray.Slice(
                i * CHUNK_TRIANGLE_BUFFER_SIZE,
                CHUNK_TRIANGLE_BUFFER_SIZE);

            NativeSlice<Vector2> chunksUVSArraySlice = ChunksUVSNativeArray.Slice(
                i * CHUNK_UV_BUFFER_SIZE,
                CHUNK_UV_BUFFER_SIZE);

            Mesh chunkMesh = new();

            chunkMesh.vertices = VectorUtils.ConvertArrayVector3IntToVector3(ref chunksVerticesArraySlice);
            chunkMesh.triangles = chunksTriangleArraySlice.ToArray();
            chunkMesh.uv = chunksUVSArraySlice.ToArray();

            ChunkCoord chunkCoord = ChunkCoordNativeArray[i];
            GameObject chunkGameObject = Instantiate(new GameObject($"Chunk - X:{chunkCoord.XCoord} / Y:{chunkCoord.YCoord}"));
            chunkGameObject.AddComponent<MeshFilter>().mesh = chunkMesh;
            chunkGameObject.AddComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            chunkGameObject.transform.SetParent(transform);

            yield return null;

            //mesh.vertices = worldChunksVerticesArraySlice.ToArray();

            //mesh.SetVertices(WorldChunksVerticesNativeArray, i * (Chunk.TOTAL_SIZE * Tile.VERTICES), (Chunk.TOTAL_SIZE * Tile.VERTICES) + Tile.VERTICES);
            //mesh.SetTriangles(trianglesArray, i * (Chunk.TOTAL_SIZE * Tile.TRIANGLES), (Chunk.TOTAL_SIZE * Tile.TRIANGLES) + Tile.TRIANGLES, 0, false);
            //mesh.SetUVs(0, WorldChunksUVSNativeArray, i * (Chunk.TOTAL_SIZE * Tile.UVS), (Chunk.TOTAL_SIZE * Tile.UVS) + Tile.UVS);

            //mesh.SetVertexBufferParams((Chunk.TOTAL_SIZE * Tile.VERTICES) + Tile.VERTICES);
            //mesh.SetVertexBufferData(
            //    WorldChunksVerticesNativeArray,
            //    i * (Chunk.TOTAL_SIZE * Tile.VERTICES),
            //    0,
            //    (Chunk.TOTAL_SIZE * Tile.VERTICES) + Tile.VERTICES,
            //    0,
            //    MeshUpdateFlags.DontRecalculateBounds);

            //mesh.SetIndexBufferParams((Chunk.TOTAL_SIZE * Tile.TRIANGLES) + Tile.TRIANGLES, IndexFormat.UInt16);
            //mesh.SetIndexBufferData(
            //    WorldChunksTrianglesNativeArray,
            //    i * (Chunk.TOTAL_SIZE * Tile.TRIANGLES),
            //    0,
            //    (Chunk.TOTAL_SIZE * Tile.TRIANGLES) + Tile.TRIANGLES,
            //    MeshUpdateFlags.DontValidateIndices);

            //SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor(0, (Chunk.TOTAL_SIZE * Tile.TRIANGLES) + Tile.TRIANGLES, MeshTopology.Triangles);
            //mesh.SetSubMesh(0, subMeshDescriptor, MeshUpdateFlags.DontRecalculateBounds);    
        }
    }

    private void OnApplicationQuit()
    {
        ChunkCoordNativeArray.Dispose();
        TileCoordNativeArray.Dispose();
        TileDataNativeArray.Dispose();
        ChunksVerticesNativeArray.Dispose();
        ChunksTrianglesNativeArray.Dispose();
        ChunksUVSNativeArray.Dispose();
    }
}
