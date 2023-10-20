using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class World : MonoBehaviour
{
    // Size in chunks.
    private const short WORLD_X_SIZE = 1024;
    private const short WORLD_Y_SIZE = 1024;

    public const short VERTEX_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.VERTICES;
    public const short TRIANGLE_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.TRIANGLES;
    public const short UV_BUFFER_SIZE = Chunk.TOTAL_SIZE * Tile.UVS;

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

        ChunksVerticesNativeArray = new((XSizeInChunks * YSizeInChunks) * VERTEX_BUFFER_SIZE, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        ChunksTrianglesNativeArray = new((XSizeInChunks * YSizeInChunks) * TRIANGLE_BUFFER_SIZE, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        ChunksUVSNativeArray = new((XSizeInChunks * YSizeInChunks) * UV_BUFFER_SIZE, 
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

        //NativeArray<JobHandle> jobDependenciesHandleNativeArray = new(4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        NativeArray<VertexAttributeDescriptor> vertexLayoutArray = new(1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        vertexLayoutArray[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

        NativeArray<VertexAttributeDescriptor> uvLayoutArray = new(1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        uvLayoutArray[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2);

        for (int index = 0; index < ChunkCoordNativeArray.Length; index++) 
        {
            NativeSlice<TileData> tileDataNativeSlice = new(TileDataNativeArray, index * Chunk.TOTAL_SIZE, Chunk.TOTAL_SIZE);

            // First start the job to create the vertices.
            CreateChunkVerticesJob createChunkVerticesJob = 
                new(ChunkCoordNativeArray[index], tileDataNativeSlice, index, ChunksVerticesNativeArray, ChunkMeshDataArray, vertexLayoutArray);
            JobHandle jobHandleCreateVertices = createChunkVerticesJob.Schedule(_jobHandleCreateWorldChunksMeshData);

            // Schedule the job to create the UVs when the job that will create the vertices finish.
            CreateChunkUVSJob createChunkUVSJob =
                new(ChunkCoordNativeArray[index], tileDataNativeSlice, index, ChunksUVSNativeArray, ChunkMeshDataArray, uvLayoutArray);
            JobHandle jobHandleCreateUVS = createChunkUVSJob.Schedule(jobHandleCreateVertices);

            // Start the job to create the triangles independently from the other.
            CreateChunkTrianglesJob createChunkTrianglesJob = 
                new(ChunkCoordNativeArray[index], tileDataNativeSlice, index, ChunksTrianglesNativeArray, ChunkMeshDataArray);
            JobHandle jobHandleCreateTriangles = createChunkTrianglesJob.Schedule(jobHandleCreateUVS);

            // Handle get all jobHandles and combine all dependencies of them before proceeding.
            //jobDependenciesHandleNativeArray[0] = _jobHandleCreateWorldChunksMeshData;
            //jobDependenciesHandleNativeArray[1] = jobHandleCreateVertices;
            //jobDependenciesHandleNativeArray[2] = jobHandleCreateTriangles;
            //jobDependenciesHandleNativeArray[3] = jobHandleCreateUVS;
            //_jobHandleCreateWorldChunksMeshData = JobHandle.CombineDependencies(jobDependenciesHandleNativeArray);
            _jobHandleCreateWorldChunksMeshData = jobHandleCreateTriangles;
        }

        _jobHandleCreateWorldChunksMeshData.Complete();
        //jobDependenciesHandleNativeArray.Dispose();

        vertexLayoutArray.Dispose();
        uvLayoutArray.Dispose();
    }

    private IEnumerator YieldedChunkMeshGeneration()
    {
        List<Mesh> meshList = new List<Mesh>(ChunkCoordNativeArray.Length);
        for (int index = 0; index < ChunkCoordNativeArray.Length; index++)
        {
            yield return new WaitWhile(() => PauseChunkMeshGeneration);

            ChunkCoord chunkCoord = ChunkCoordNativeArray[index];
            Mesh mesh = new();

            if (!UseAdvancedMeshAPI)
            {
                NativeSlice<int> chunkTriangleArraySlice = ChunksTrianglesNativeArray.Slice(
                    index * TRIANGLE_BUFFER_SIZE,
                    TRIANGLE_BUFFER_SIZE);

                NativeSlice<Vector2> chunkUVSArraySlice = ChunksUVSNativeArray.Slice(
                    index * UV_BUFFER_SIZE,
                    UV_BUFFER_SIZE);

                mesh.SetVertices(ChunksVerticesNativeArray, index * VERTEX_BUFFER_SIZE, VERTEX_BUFFER_SIZE, MeshUpdateFlags.DontRecalculateBounds);
                mesh.triangles = chunkTriangleArraySlice.ToArray();
                mesh.uv = chunkUVSArraySlice.ToArray();

                GameObject chunkGameObject = Instantiate(
                    new GameObject($"Chunk - X:{chunkCoord.XCoord} / Y:{chunkCoord.YCoord}"),
                    new Vector3(chunkCoord.XCoord, chunkCoord.YCoord), 
                    Quaternion.identity, gameObject.transform);
                chunkGameObject.AddComponent<MeshFilter>().mesh = mesh;
                chunkGameObject.AddComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
            else
            {
                Mesh.MeshData meshData = ChunkMeshDataArray[index];
                meshData.subMeshCount = 1;
                meshData.SetSubMesh(
                    0,
                    new SubMeshDescriptor(0, TRIANGLE_BUFFER_SIZE, MeshTopology.Triangles),
                    MeshUpdateFlags.DontRecalculateBounds
                );
                GameObject chunkGameObject = Instantiate(
                  new GameObject($"Chunk - X:{chunkCoord.XCoord} / Y:{chunkCoord.YCoord}"),
                  new Vector3(chunkCoord.XCoord, chunkCoord.YCoord),
                  Quaternion.identity, gameObject.transform);
                chunkGameObject.AddComponent<MeshFilter>().mesh = mesh;
                chunkGameObject.AddComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                meshList.Add(mesh);
            }

            yield return null;
        }

        Mesh.ApplyAndDisposeWritableMeshData(ChunkMeshDataArray, meshList, MeshUpdateFlags.DontValidateIndices);
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
