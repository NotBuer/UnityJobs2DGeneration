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
    private const ushort WORLD_X_SIZE = 1024;
    private const ushort WORLD_Y_SIZE = 1024;

    [Header("Debugging")]

    [Range(0, WORLD_X_SIZE)]
    [SerializeField]
    private ushort _XSizeInChunks;

    [Range(0, WORLD_Y_SIZE)]
    [SerializeField]
    private ushort _YSizeInChunks;

    [SerializeField] private bool _uploadMeshToGPUOnMeshBuilt = false;
    [SerializeField] private bool _useAdvancedMeshAPI = false;
    [SerializeField] private bool _pauseChunkMeshGeneration = false;
    [SerializeField] private Material _atlasMaterial;

    private short _worldMiddleX = 0;
    private short _worldMiddleY = 0;

    public NativeArray<ChunkCoord> ChunkCoordNativeArray { get; set; }
    public NativeArray<TileCoord> TileCoordNativeArray { get; set; }
    public NativeArray<TileData> TileDataNativeArray { get; set; }
    public NativeArray<Vector3> ChunksVerticesNativeArray { get; set; }
    public NativeArray<int> ChunksIndicesNativeArray { get; set; }
    public NativeArray<Vector2> ChunksUVSNativeArray { get; set; }
    public Mesh.MeshDataArray ChunkMeshDataArray { get; set; }

    private JobHandle _jobHandleCreateChunkCoord; 
    private JobHandle _jobHandleCreateTileCoord;
    private JobHandle _jobHandleCreateTileData;
    private JobHandle _jobHandleWorldDataGenerated;
    private JobHandle _jobHandleCreateWorldChunksMeshData;

    private bool _createChunkCoordsOnce = false;
    private bool _createTileCoordsOnce = false;
    private bool _createTileDataOnce = false;
    private bool _rawWorldDataGenerated = false;
    private bool _createWorldChunksMeshDataOnce = false;


    private void Awake() 
    {
        ChunkCoordNativeArray = new(_XSizeInChunks * _YSizeInChunks, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        TileCoordNativeArray = new(Chunk.TOTAL_SIZE * ChunkCoordNativeArray.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        TileDataNativeArray = new(Chunk.TOTAL_SIZE * ChunkCoordNativeArray.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        if (_useAdvancedMeshAPI)
        {
            ChunksVerticesNativeArray = new(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            ChunksIndicesNativeArray = new(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            ChunksUVSNativeArray = new(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            ChunkMeshDataArray = Mesh.AllocateWritableMeshData(ChunkCoordNativeArray.Length);
        }
        else
        {
            ChunksVerticesNativeArray = new((_XSizeInChunks * _YSizeInChunks) * VertexLayout.VERTEX_BUFFER_SIZE,
                Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            ChunksIndicesNativeArray = new((_XSizeInChunks * _YSizeInChunks) * VertexLayout.INDEX_BUFFER_SIZE,
                Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            ChunksUVSNativeArray = new((_XSizeInChunks * _YSizeInChunks) * VertexLayout.VERTEX_BUFFER_SIZE,
                Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            ChunkMeshDataArray = Mesh.AllocateWritableMeshData(0);
        }

        _worldMiddleX = (short)(_XSizeInChunks / 2);
        _worldMiddleY = (short)(_YSizeInChunks / 2);
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

        _jobHandleWorldDataGenerated = JobHandle.CombineDependencies(_jobHandleCreateChunkCoord, _jobHandleCreateTileCoord, _jobHandleCreateTileData);
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
        if (_jobHandleWorldDataGenerated.IsCompleted && !_rawWorldDataGenerated)
        {
            _rawWorldDataGenerated = true;
            _jobHandleWorldDataGenerated.Complete();
            GenerateChunkMeshData();
        }

        if (_jobHandleCreateWorldChunksMeshData.IsCompleted && !_createWorldChunksMeshDataOnce)
        {
            _createWorldChunksMeshDataOnce = true;
            _jobHandleCreateWorldChunksMeshData.Complete();
            StartCoroutine(YieldedChunkMeshGeneration());

            Debug.Log("WORLD - World Chunks Mesh Data generated successfully!");
            if(!_useAdvancedMeshAPI)
            {
                Debug.Log($"WORLD - Vertices array size: {ChunksVerticesNativeArray.Length}");
                Debug.Log($"WORLD - Triangles array size: {ChunksIndicesNativeArray.Length}");
                Debug.Log($"WORLD - UVS array size: {ChunksUVSNativeArray.Length}");
            }
        }
    }

    private void GenerateChunkMeshData()
    {
        Debug.Log("WORLD - Scheduling generation of World Chunks Mesh Data...");
        _jobHandleCreateWorldChunksMeshData = _jobHandleWorldDataGenerated;

        NativeArray<JobHandle> jobDependenciesHandleNativeArray = new(4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        NativeArray<VertexAttributeDescriptor> bufferLayout = new(VertexLayout.DefinedVertexLayout(), Allocator.TempJob);

        for (int i = 0; i < ChunkCoordNativeArray.Length; i++) 
        {
            NativeSlice<TileData> tileDataNativeSlice = TileDataNativeArray.Slice(i * Chunk.TOTAL_SIZE, Chunk.TOTAL_SIZE);

            // First start the job to create the vertices.
            CreateChunkVerticesJob createChunkVerticesJob = 
                new(ChunkCoordNativeArray[i], tileDataNativeSlice, i, ChunksVerticesNativeArray, ChunkMeshDataArray, bufferLayout, _useAdvancedMeshAPI);
            JobHandle jobHandleCreateVertices = createChunkVerticesJob.Schedule(_jobHandleCreateWorldChunksMeshData);


            // Schedule the job to create the UVs when the job that will create the vertices finish.
            CreateChunkUVSJob createChunkUVSJob =
                new(ChunkCoordNativeArray[i], tileDataNativeSlice, i, ChunksUVSNativeArray, ChunkMeshDataArray, _useAdvancedMeshAPI);
            JobHandle jobHandleCreateUVS = createChunkUVSJob.Schedule(jobHandleCreateVertices);


            // Start the job to create the triangles independently from the other.
            CreateChunkIndicesJob createChunkIndicesJob = 
                new(ChunkCoordNativeArray[i], tileDataNativeSlice, i, ChunksIndicesNativeArray, ChunkMeshDataArray, _useAdvancedMeshAPI);
            JobHandle jobHandleCreateIndices = createChunkIndicesJob.Schedule(jobHandleCreateUVS);

            // Handle get all jobHandles and combine all dependencies of them before proceeding.
            jobDependenciesHandleNativeArray[0] = _jobHandleCreateWorldChunksMeshData;
            jobDependenciesHandleNativeArray[1] = jobHandleCreateVertices;
            jobDependenciesHandleNativeArray[2] = jobHandleCreateUVS;
            jobDependenciesHandleNativeArray[3] = jobHandleCreateIndices;
            _jobHandleCreateWorldChunksMeshData = JobHandle.CombineDependencies(jobDependenciesHandleNativeArray);
        }

        _jobHandleCreateWorldChunksMeshData.Complete();
        bufferLayout.Dispose(_jobHandleCreateWorldChunksMeshData);
        jobDependenciesHandleNativeArray.Dispose();
    }

    private IEnumerator YieldedChunkMeshGeneration()
    {
        List<Mesh> meshList = new(ChunkCoordNativeArray.Length);
        for (int index = 0; index < ChunkCoordNativeArray.Length; index++)
        {
            yield return new WaitWhile(() => _pauseChunkMeshGeneration);

            Mesh mesh = new();

            if (_useAdvancedMeshAPI)
            {
                Mesh.MeshData meshData = ChunkMeshDataArray[index];
                meshData.subMeshCount = 1;
                meshData.SetSubMesh(
                    0,
                    new SubMeshDescriptor(0, VertexLayout.INDEX_BUFFER_SIZE, MeshTopology.Triangles),
                    MeshUpdateFlags.DontValidateIndices
                );
                meshList.Add(mesh);
            }
            else
            {
                NativeSlice<int> chunkTriangleArraySlice = ChunksIndicesNativeArray.Slice(
                    index * VertexLayout.INDEX_BUFFER_SIZE,
                    VertexLayout.INDEX_BUFFER_SIZE
                );

                NativeSlice<Vector2> chunkUVSArraySlice = ChunksUVSNativeArray.Slice(
                    index * VertexLayout.VERTEX_BUFFER_SIZE,
                    VertexLayout.VERTEX_BUFFER_SIZE
                );

                mesh.SetVertices(
                    ChunksVerticesNativeArray, 
                    index * VertexLayout.VERTEX_BUFFER_SIZE, 
                    VertexLayout.VERTEX_BUFFER_SIZE, 
                    MeshUpdateFlags.DontValidateIndices
                );
                mesh.triangles = chunkTriangleArraySlice.ToArray();
                mesh.uv = chunkUVSArraySlice.ToArray();
            }

            mesh.RecalculateBounds();

            ChunkCoord chunkCoord = ChunkCoordNativeArray[index];
            GameObject chunkGameObject = Instantiate(
              new GameObject($"Chunk - X:{chunkCoord.XCoord} / Y:{chunkCoord.YCoord}"),
              new Vector3(chunkCoord.XCoord, chunkCoord.YCoord),
              Quaternion.identity, gameObject.transform);

            MeshFilter meshFilter = chunkGameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = _atlasMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            yield return null;

            if (_uploadMeshToGPUOnMeshBuilt) mesh.UploadMeshData(true);
            else mesh.UploadMeshData(false);
        }

        if (_useAdvancedMeshAPI)
            Mesh.ApplyAndDisposeWritableMeshData(ChunkMeshDataArray, meshList, MeshUpdateFlags.DontValidateIndices);
    }

    private void OnApplicationQuit()
    {
        ChunkCoordNativeArray.Dispose();
        TileCoordNativeArray.Dispose();
        TileDataNativeArray.Dispose();
        if (!_useAdvancedMeshAPI)
        {
            ChunksVerticesNativeArray.Dispose();
            ChunksIndicesNativeArray.Dispose();
            ChunksUVSNativeArray.Dispose();
        }
    }
}
