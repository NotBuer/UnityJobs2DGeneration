using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class World : MonoBehaviour
{
    // Size in chunks.
    private const short WORLD_X_SIZE = 1024;
    private const short WORLD_Y_SIZE = 1024;

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

    private short _worldMiddleX = 0;
    private short _worldMiddleY = 0;

    public NativeArray<ChunkCoord> ChunkCoordNativeArray { get; set; }
    public NativeArray<TileCoord> TileCoordNativeArray { get; set; }
    public NativeArray<TileData> TileDataNativeArray { get; set; }
    public NativeArray<Vector2Int> WorldChunksVerticesNativeArray { get; set; }
    public NativeArray<int> WorldChunksTrianglesNativeArray { get; set; }
    public NativeArray<Vector2> WorldChunksUVSNativeArray { get; set; }

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
        TileCoordNativeArray = new((Chunk.TOTAL_SIZE + 1) * ChunkCoordNativeArray.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        TileDataNativeArray = new((Chunk.TOTAL_SIZE + 1) * ChunkCoordNativeArray.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        WorldChunksVerticesNativeArray = new((XSizeInChunks * YSizeInChunks) * Chunk.TOTAL_SIZE * Tile.VERTICES, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        WorldChunksTrianglesNativeArray = new((XSizeInChunks * YSizeInChunks) * Chunk.TOTAL_SIZE * Tile.TRIANGLES, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        WorldChunksUVSNativeArray = new((XSizeInChunks * YSizeInChunks) * Chunk.TOTAL_SIZE * Tile.UVS, 
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        _worldMiddleX = (short)(XSizeInChunks / 2);
        _worldMiddleY = (short)(YSizeInChunks / 2);
    }

    void Start()
    {
        CreateChunkCoordJob createChunksJob = new CreateChunkCoordJob
        {
            chunksCoordsNativeArray = ChunkCoordNativeArray,
            worldMiddleX = _worldMiddleX,
            worldMiddleY = _worldMiddleY,
        };
        _jobHandleCreateChunkCoord = createChunksJob.Schedule();
        Debug.Log("WORLD - Scheduling generation of ChunkCoord...");

        CreateTileCoordJob createTileCoordJob = new CreateTileCoordJob
        {
            chunksInTheWorld = ChunkCoordNativeArray.Length - 1,
            tileCoordNativeArray = TileCoordNativeArray
        };
        _jobHandleCreateTileCoord = createTileCoordJob.Schedule(_jobHandleCreateChunkCoord);
        Debug.Log("WORLD - Scheduling generation of TileCoord...");

        CreateTileDataJob createTileDataJob = new CreateTileDataJob
        {
            chunksInTheWorld = ChunkCoordNativeArray.Length - 1,
            tileDataNativeArray = TileDataNativeArray
        };
        _jobHandleCreateTileData = createTileDataJob.Schedule(_jobHandleCreateChunkCoord);
        Debug.Log("WORLD - Scheduling generation of TileData...");

        _jobHandleRawWorldDataGenerated = JobHandle.CombineDependencies(_jobHandleCreateChunkCoord, _jobHandleCreateTileCoord, _jobHandleCreateTileData);

        //CreateChunkMeshDataJob createChunkMeshDataJob = new CreateChunkMeshDataJob
        //{
        //    chunkCoordNativeArray = ChunkCoordNativeArray,
        //    tileDataNativeArray = TileDataNativeArray,
        //    worldChunksVerticesNativeArray = WorldChunksVerticesNativeArray,
        //    worldChunksTrianglesNativeArray = WorldChunksTrianglesNativeArray,
        //    worldChunksUVSNativeArray = WorldChunksUVSNativeArray
        //};
        //_jobHandleCreateWorldChunksMeshData = createChunkMeshDataJob.Schedule(_jobHandleCombineTileCoordTileData);
        //Debug.Log("WORLD - Scheduling generation of World Chunks Mesh Data...");
    }

    private void LateUpdate()
    {
        if (_jobHandleCreateChunkCoord.IsCompleted && !_createChunkCoordsOnce)
        {
            _createChunkCoordsOnce = true;
            _jobHandleCreateChunkCoord.Complete();
            Debug.Log("WORLD - ChunkCoords generated successfully!");
            if (DebugLogChunkCoordGeneration) StartCoroutine(DebugWorldGeneration.DebugCreateChunkCoord(ChunkCoordNativeArray));
        }

        if (_jobHandleCreateTileCoord.IsCompleted && !_createTileCoordsOnce)
        {
            _createTileCoordsOnce = true;
            _jobHandleCreateTileCoord.Complete();
            Debug.Log("WORLD - TileCoord generated successfully!");
            if (DebugLogTileCoordGeneration) StartCoroutine(DebugWorldGeneration.DebugCreateTileCoord(TileCoordNativeArray));
        }

        if (_jobHandleCreateTileData.IsCompleted && !_createTileDataOnce)
        {
            _createTileDataOnce = true;
            _jobHandleCreateTileData.Complete();
            Debug.Log("WORLD - TileData generated successfully!");
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
            Debug.Log("WORLD - World Chunks Mesh Data generated successfully!");
            if (DebugLogWorldChunksVerticesData) StartCoroutine(DebugWorldGeneration.DebugWorldChunksVerticesData(WorldChunksVerticesNativeArray));
            if (DebugLogWorldChunksTrianglesData) StartCoroutine(DebugWorldGeneration.DebugWorldChunksTrianglesData(WorldChunksTrianglesNativeArray));
            if (DebugLogWorldChunksUVSData) StartCoroutine(DebugWorldGeneration.DebugWorldChunksUVSData(WorldChunksUVSNativeArray));
        }
    }

    private void GenerateChunkMeshData()
    {
        Debug.Log("WORLD - Scheduling generation of World Chunks Mesh Data...");
        _jobHandleCreateWorldChunksMeshData = _jobHandleRawWorldDataGenerated;

        for (int i = 0; i < ChunkCoordNativeArray.Length; i++) 
        {
            NativeSlice<TileData> tileDataNativeSlice = new(TileDataNativeArray, (i * Chunk.TOTAL_SIZE) + i, Chunk.TOTAL_SIZE);
            CreateChunkVerticesJob createChunkVerticesJob = new CreateChunkVerticesJob()
            {
                chunkCoord = ChunkCoordNativeArray[i],
                tileDataNativeSlice = tileDataNativeSlice,
                stride = i,
                worldChunksVerticesNativeArray = WorldChunksVerticesNativeArray,
            };
            JobHandle jobHandleCreateVertices = createChunkVerticesJob.Schedule(_jobHandleCreateWorldChunksMeshData);

            CreateChunkTrianglesJob createChunkTrianglesJob = new CreateChunkTrianglesJob()
            {
                chunkCoord = ChunkCoordNativeArray[i],
                tileDataNativeSlice = tileDataNativeSlice,
                stride = i,
                worldChunksTrianglesArray = WorldChunksTrianglesNativeArray
            };
            JobHandle jobHandleCreateTriangles = createChunkTrianglesJob.Schedule(_jobHandleCreateWorldChunksMeshData);

            _jobHandleCreateWorldChunksMeshData = JobHandle.CombineDependencies(_jobHandleCreateWorldChunksMeshData, jobHandleCreateVertices, jobHandleCreateTriangles);
        }

        _jobHandleCreateWorldChunksMeshData.Complete();
    }

    private void OnApplicationQuit()
    {
        ChunkCoordNativeArray.Dispose();
        TileCoordNativeArray.Dispose();
        TileDataNativeArray.Dispose();
        WorldChunksVerticesNativeArray.Dispose();
        WorldChunksTrianglesNativeArray.Dispose();
        WorldChunksUVSNativeArray.Dispose();
    }
}
