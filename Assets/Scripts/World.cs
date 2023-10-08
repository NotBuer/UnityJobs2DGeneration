using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    public NativeHashMap<ChunkCoord, ChunkData> WorldNativeHashMap { get; set; }

    //private CancellationTokenSource _tokenSource = null;
    //public ConcurrentDictionary<ChunkCoord, ChunkData> WorldDataDictionary { get; set; } = null;
    //public ConcurrentQueue<ChunkData> ChunksReadyToBuildMeshQueue { get; set; } = null;

    private bool _createOnce = false;
    private JobHandle _jobHandle;
    private TileMeshData _tileMeshData;

    [BurstCompile]
    private struct TileMeshGenerationJob : IJob
    {
        public TileMeshData tileMeshData;

        public void Execute()
        {
            Tile.CreateVertices(ref tileMeshData.vertices);
            Tile.CreateUvs(ref tileMeshData.uvs);
            Tile.CreateTriangles(ref tileMeshData.triangles);
        }
    }

    private void Awake()
    {
        //_tokenSource = new CancellationTokenSource();
    }

    void Start()
    {
        //WorldDataDictionary = new(Environment.ProcessorCount, XSizeInChunks * YSizeInChunks);
        //ChunksReadyToBuildMeshQueue = new();
        _worldMiddleX = (short)(XSizeInChunks / 2);
        _worldMiddleY = (short)(YSizeInChunks / 2);

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

        //HandleCreateChunksInParallel();

        //while (ChunksReadyToBuildMeshQueue.Count > 0)
        //{
        //    if (ChunksReadyToBuildMeshQueue.TryDequeue(out ChunkData chunkData))
        //    {
        //        GameObject chunkObj = Instantiate(
        //            new GameObject($"X:{chunkData.ChunkCoord.XCoord} / Y:{chunkData.ChunkCoord.YCoord}"),
        //            gameObject.transform, true);

        //        Mesh mesh = new();

        //        mesh.SetVertices(Tile.CreateVertices());
        //        mesh.SetUVs(0, Tile.CreateUvs());
        //        mesh.SetTriangles(Tile.CreateTriangles(), 0);

        //        MeshFilter meshfilter = chunkObj.AddComponent<MeshFilter>();
        //        meshfilter.mesh = mesh;

        //        MeshRenderer meshRenderer = chunkObj.AddComponent<MeshRenderer>();
        //        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //    }
        //}
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

    private void OnApplicationQuit()
    {
        //_tokenSource.Cancel(false);
    }
}
