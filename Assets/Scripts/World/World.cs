using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class World : MonoBehaviour
{
    // Size in chunks.
    private const short WORLD_X_SIZE = 1024;
    private const short WORLD_Y_SIZE = 1024;

    public const short CHUNK_VERTEX_ARRAY_LENGTH = Chunk.TOTAL_SIZE * Tile.VERTICES;
    public const short CHUNK_TRIANGLE_ARRAY_LENGTH = Chunk.TOTAL_SIZE * Tile.TRIANGLES;
    public const short CHUNK_UV_ARRAY_LENGTH = Chunk.TOTAL_SIZE * Tile.UVS;

    private const byte FLOAT32_MEMORY_SIZE = 4; // 4 Bytes.
    private const byte VERTEX_DIMENSION = 3;
    private const byte UV_DIMENSION = 2;
    private const short MESH_VERTEX_BUFFER_SIZE = (VERTEX_DIMENSION + UV_DIMENSION) * FLOAT32_MEMORY_SIZE * CHUNK_VERTEX_ARRAY_LENGTH; // 20 Kilobytes equivalent.

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
    public NativeArray<Vector3> ChunksVerticesNativeArray { get; set; }
    public NativeArray<int> ChunksTrianglesNativeArray { get; set; }
    public NativeArray<Vector2> ChunksUVSNativeArray { get; set; }
    public Mesh.MeshDataArray ChunkMeshDataArray { get; set; }

    private JobHandle _jobHandleCreateChunkCoord; 
    private JobHandle _jobHandleCreateTileCoord;
    private JobHandle _jobHandleCreateTileData;
    private JobHandle _jobHandleRawWorldDataGenerated;
    private JobHandle _jobHandleCreateWorldChunksMeshData;
    private JobHandle _jobHandleCreateChunkMesh;

    private bool _createChunkCoordsOnce = false;
    private bool _createTileCoordsOnce = false;
    private bool _createTileDataOnce = false;
    private bool _rawWorldDataGenerated = false;
    private bool _createWorldChunksMeshDataOnce = false;

    private void Awake() 
    {
        ChunkCoordNativeArray = new(XSizeInChunks * YSizeInChunks, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        TileCoordNativeArray = new(Chunk.TOTAL_SIZE * ChunkCoordNativeArray.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        TileDataNativeArray = new(Chunk.TOTAL_SIZE * ChunkCoordNativeArray.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        ChunksVerticesNativeArray = new((XSizeInChunks * YSizeInChunks) * CHUNK_VERTEX_ARRAY_LENGTH, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        ChunksTrianglesNativeArray = new((XSizeInChunks * YSizeInChunks) * CHUNK_TRIANGLE_ARRAY_LENGTH, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        ChunksUVSNativeArray = new((XSizeInChunks * YSizeInChunks) * CHUNK_UV_ARRAY_LENGTH, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        ChunkMeshDataArray = Mesh.AllocateWritableMeshData(ChunkCoordNativeArray.Length);

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

    private void Update()
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

        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        for (int meshIndex = 0; meshIndex < ChunkMeshDataArray.Length; meshIndex++)
        {
            ChunkMeshDataArray[meshIndex].SetVertexBufferParams(MESH_VERTEX_BUFFER_SIZE, layout);
            //ChunkMeshDataArray[meshIndex].SetIndexBufferParams(CHUNK_TRIANGLE_ARRAY_LENGTH, IndexFormat.UInt16);
        }

        var test = ChunkMeshDataArray[0].GetVertexData<Vector3>();

        NativeArray<JobHandle> jobDependenciesHandleNativeArray = new NativeArray<JobHandle>(4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        for (int index = 0; index < ChunkCoordNativeArray.Length; index++) 
        {
            NativeSlice<TileData> tileDataNativeSlice = new(TileDataNativeArray, index * Chunk.TOTAL_SIZE, Chunk.TOTAL_SIZE);

            // First start the job to create the vertices.
            CreateChunkVerticesJob createChunkVerticesJob = 
                new(ChunkCoordNativeArray[index], tileDataNativeSlice, index, ChunksVerticesNativeArray, ChunkMeshDataArray);
            JobHandle jobHandleCreateVertices = createChunkVerticesJob.Schedule(_jobHandleCreateWorldChunksMeshData);

            // Schedule the job to create the UVs when the job that will create the vertices finish.
            CreateChunkUVSJob createChunkUVSJob =
                new(ChunkCoordNativeArray[index], tileDataNativeSlice, index, ChunksUVSNativeArray, ChunkMeshDataArray);
            JobHandle jobHandleCreateUVS = createChunkUVSJob.Schedule(jobHandleCreateVertices);

            // Start the job to create the triangles independently from the other.
            CreateChunkTrianglesJob createChunkTrianglesJob = 
                new(ChunkCoordNativeArray[index], tileDataNativeSlice, index, ChunksTrianglesNativeArray);
            JobHandle jobHandleCreateTriangles = createChunkTrianglesJob.Schedule(_jobHandleCreateWorldChunksMeshData);

            // Handle get all jobHandles and combine all dependencies of them before proceeding.
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

            Mesh mesh = new();

            if (!UseAdvancedMeshAPI)
            {
                //NativeSlice<Vector3> chunkVerticesArraySlice = ChunksVerticesNativeArray.Slice(
                //i * CHUNK_VERTEX_ARRAY_LENGTH,
                //CHUNK_VERTEX_ARRAY_LENGTH);

                NativeSlice<int> chunkTriangleArraySlice = ChunksTrianglesNativeArray.Slice(
                    i * CHUNK_TRIANGLE_ARRAY_LENGTH,
                    CHUNK_TRIANGLE_ARRAY_LENGTH);

                NativeSlice<Vector2> chunkUVSArraySlice = ChunksUVSNativeArray.Slice(
                    i * CHUNK_UV_ARRAY_LENGTH,
                CHUNK_UV_ARRAY_LENGTH);

                mesh.SetVertices(ChunksVerticesNativeArray, i * CHUNK_VERTEX_ARRAY_LENGTH, CHUNK_VERTEX_ARRAY_LENGTH, MeshUpdateFlags.DontRecalculateBounds);
                mesh.triangles = chunkTriangleArraySlice.ToArray();
                mesh.uv = chunkUVSArraySlice.ToArray();

                ChunkCoord chunkCoord = ChunkCoordNativeArray[i];
                GameObject chunkGameObject = Instantiate(
                    new GameObject($"Chunk - X:{chunkCoord.XCoord} / Y:{chunkCoord.YCoord}"),
                    new Vector3(chunkCoord.XCoord, chunkCoord.YCoord), 
                    Quaternion.identity, gameObject.transform);
                chunkGameObject.AddComponent<MeshFilter>().mesh = mesh;
                chunkGameObject.AddComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
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
        ChunkMeshDataArray.Dispose();
    }
}
