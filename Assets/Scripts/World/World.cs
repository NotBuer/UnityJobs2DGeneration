using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class World : MonoBehaviour
{
    // Size in chunks.
    private const short WORLD_X_SIZE = 1024;
    private const short WORLD_Y_SIZE = 256;

    [Header("Debugging")]

    [Range(0, WORLD_X_SIZE)]
    [SerializeField]
    private short XSizeInChunks;

    [Range(0, WORLD_Y_SIZE)]
    [SerializeField]
    private short YSizeInChunks;

    private short _worldMiddleX = 0;
    private short _worldMiddleY = 0;

    //public NativeHashMap<ChunkCoord, ChunkData> WorldHashMap { get; set; }

    public NativeArray<ChunkCoord> ChunkCoordNativeArray { get; set; }
    public NativeArray<TileCoord> TileCoordNativeArray { get; set; }
    public NativeArray<TileData> TileDataNativeArray { get; set; }

    private JobHandle _jobHandleCreateChunkCoords;
    private JobHandle _jobHandleCreateTileCoords;
    private JobHandle _jobHandleCreateTileData;


    private bool createOnce = false;

    //[BurstCompile]
    //private struct TileMeshGenerationJob : IJob
    //{
    //    public TileMeshData tileMeshData;

    //    public void Execute()
    //    {
    //        Tile.CreateVertices(ref tileMeshData.vertices);
    //        Tile.CreateUvs(ref tileMeshData.uvs);
    //        Tile.CreateTriangles(ref tileMeshData.triangles);
    //    }
    //}

    private void Awake()
    {
        //WorldHashMap = new(XSizeInChunks * YSizeInChunks, Allocator.TempJob);

        ChunkCoordNativeArray = new(XSizeInChunks * YSizeInChunks, Allocator.Persistent);

        _worldMiddleX = (short)(XSizeInChunks / 2);
        _worldMiddleY = (short)(YSizeInChunks / 2);
    }

    void Start()
    {
        CreateChunkCoordJob createChunksJob = new CreateChunkCoordJob
        {
            chunksCoords = ChunkCoordNativeArray,
            worldMiddleX = _worldMiddleX,
            worldMiddleY = _worldMiddleY,
        };

        _jobHandleCreateChunkCoords = createChunksJob.Schedule();

        // (TEST) -> Generate single tile on worker thread.
        //_tileMeshData = new TileMeshData(Allocator.TempJob);

        //TileMeshGenerationJob tileMeshGenerationJob = new TileMeshGenerationJob
        //{
        //    tileMeshData = _tileMeshData
        //};

        //_jobHandle = tileMeshGenerationJob.Schedule();
    }

    private void Update()
    {
        if (_jobHandleCreateChunkCoords.IsCompleted && !createOnce)
        {
            _jobHandleCreateChunkCoords.Complete();

            //CreateTileJob createTileJob = new CreateTileJob();
            //createTileJob.Schedule();

            createOnce = true;

            //foreach(var chunkCoord in ChunkCoordsNativeArray)
            //{
            //    Debug.Log($"Chunk: " +
            //        $"XCoord {chunkCoord.XCoord} / " +
            //        $"YCoord {chunkCoord.YCoord}");
            //}
        }

        //if (_jobHandle.IsCompleted && !_createOnce)
        //{
        //    _jobHandle.Complete();

        //    Mesh mesh = new();

        //    mesh.vertices = _tileMeshData.vertices.ToArray();
        //    mesh.uv = _tileMeshData.uvs.ToArray();
        //    mesh.triangles = _tileMeshData.triangles.ToArray();

        //    //mesh.RecalculateBounds();

        //    GameObject tileObj = Instantiate(new GameObject(), transform);

        //    MeshFilter meshFilter = tileObj.AddComponent<MeshFilter>();
        //    meshFilter.mesh = mesh;

        //    MeshRenderer meshRenderer = tileObj.AddComponent<MeshRenderer>();
        //    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //    meshRenderer.receiveShadows = false;

        //    _tileMeshData.vertices.Dispose();
        //    _tileMeshData.uvs.Dispose();
        //    _tileMeshData.triangles.Dispose();

        //    _createOnce = true;
        //}
    }

    private void OnApplicationQuit()
    {
        //WorldHashMap.Dispose();
        ChunkCoordNativeArray.Dispose();
    }

    //private async void HandleCreateChunksInParallel()
    //{
    //    await ParallelCreateWorldChunks();
    //}

    //private async Task ParallelCreateWorldChunks()
    //{
    //    await Task.Run(async () =>
    //    {
    //        // TODO: Cache 'size multiplication' when using this outside of development environment...
    //        while (WorldDataDictionary.Keys.Count < (XSizeInChunks * YSizeInChunks))
    //        {
    //            for (short y = (short)-_worldMiddleY; y <= _worldMiddleY; y++)
    //            {
    //                for (short x = (short)-_worldMiddleX; x <= _worldMiddleX; x++)
    //                {
    //                    ChunkCoord chunkCoord = new ChunkCoord(x, y);
    //                    ChunkData chunkData = new ChunkData(chunkCoord, new(Environment.ProcessorCount, Chunk.TOTAL_SIZE));

    //                    WorldDataDictionary.TryAdd(chunkCoord, chunkData);

    //                    await chunkData.ParallelCreateChunkData(_tokenSource);
    //                    ChunksReadyToBuildMeshQueue.Enqueue(chunkData);

    //                    //Debug.Log($"New Chunk At -- X: {x} / Y: {y}");
    //                }
    //            }
    //        }
    //    }, _tokenSource.Token);
    //}
}
