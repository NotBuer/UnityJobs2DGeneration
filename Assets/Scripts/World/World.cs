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

    [SerializeField] private bool UseAdvancedMeshAPI = false;
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
    }

    private void LateUpdate()
    {
        if (_jobHandleCreateChunkCoord.IsCompleted && !_createChunkCoordsOnce)
        {
            _createChunkCoordsOnce = true;
            _jobHandleCreateChunkCoord.Complete();

            Debug.Log("WORLD - ChunkCoords generated successfully!");
            Debug.Log($"WORLD - ChunkCoords array size: {ChunkCoordNativeArray.Length}");
        }

        if (_jobHandleCreateTileCoord.IsCompleted && !_createTileCoordsOnce)
        {
            _createTileCoordsOnce = true;
            _jobHandleCreateTileCoord.Complete();

            Debug.Log("WORLD - TileCoord generated successfully!");
            Debug.Log($"WORLD - TileCoord array size: {TileCoordNativeArray.Length}");
        }

        if (_jobHandleCreateTileData.IsCompleted && !_createTileDataOnce)
        {
            _createTileDataOnce = true;
            _jobHandleCreateTileData.Complete();

            Debug.Log("WORLD - TileData generated successfully!");
            Debug.Log($"WORLD - TileData array size: {TileDataNativeArray.Length}");
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
        for (int i = 0; i < ChunkCoordNativeArray.Length; i++)
        {
            yield return new WaitWhile(() => PauseChunkMeshGeneration);

            if (!UseAdvancedMeshAPI)
            {
                NativeSlice<Vector3Int> chunksVerticesArraySlice = ChunksVerticesNativeArray.Slice(
                i * CHUNK_VERTEX_BUFFER_SIZE,
                CHUNK_VERTEX_BUFFER_SIZE);

                NativeSlice<int> chunksTriangleArraySlice = ChunksTrianglesNativeArray.Slice(
                    i * CHUNK_TRIANGLE_BUFFER_SIZE,
                    CHUNK_TRIANGLE_BUFFER_SIZE);

                NativeSlice<Vector2> chunksUVSArraySlice = ChunksUVSNativeArray.Slice(
                    i * CHUNK_UV_BUFFER_SIZE,
                    CHUNK_UV_BUFFER_SIZE);

                Mesh chunkMesh = new()
                {
                    vertices = VectorUtils.ConvertArrayVector3IntToVector3(ref chunksVerticesArraySlice),
                    triangles = chunksTriangleArraySlice.ToArray(),
                    uv = chunksUVSArraySlice.ToArray()
                };

                ChunkCoord chunkCoord = ChunkCoordNativeArray[i];
                GameObject chunkGameObject = Instantiate(
                    new GameObject($"Chunk - X:{chunkCoord.XCoord} / Y:{chunkCoord.YCoord}"),
                    new Vector3(chunkCoord.XCoord, chunkCoord.YCoord), 
                    Quaternion.identity, gameObject.transform);
                chunkGameObject.AddComponent<MeshFilter>().mesh = chunkMesh;
                chunkGameObject.AddComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                //chunkGameObject.transform.SetParent(transform);
            } 
            else
            {
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

            yield return null;
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
