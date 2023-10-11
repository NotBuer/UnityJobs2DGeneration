using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
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

    private short _worldMiddleX = 0;
    private short _worldMiddleY = 0;

    public NativeArray<ChunkCoord> ChunkCoordNativeArray { get; set; }
    public NativeArray<TileCoord> TileCoordNativeArray { get; set; }
    public NativeArray<TileData> TileDataNativeArray { get; set; }

    private JobHandle _jobHandleCreateChunkCoord; 
    private JobHandle _jobHandleCreateTileCoord;
    private JobHandle _jobHandleCreateTileData;

    private bool _createChunkCoordsOnce = false;
    private bool _createTileCoordsOnce = false;
    private bool _createTileDataOnce = false;

    private void Awake()
    {
        ChunkCoordNativeArray = new(XSizeInChunks * YSizeInChunks, Allocator.Persistent);
        TileCoordNativeArray = new((Chunk.TOTAL_SIZE + 1) * ChunkCoordNativeArray.Length, Allocator.Persistent);
        TileDataNativeArray = new((Chunk.TOTAL_SIZE + 1) * ChunkCoordNativeArray.Length, Allocator.Persistent);

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

        CreateTileCoordJob createTileCoordJob = new CreateTileCoordJob
        {
            chunksInTheWorld = ChunkCoordNativeArray.Length - 1,
            tileCoordNativeArray = TileCoordNativeArray
        };
        _jobHandleCreateTileCoord = createTileCoordJob.Schedule(_jobHandleCreateChunkCoord);

        CreateTileDataJob createTileDataJob = new CreateTileDataJob
        {
            chunksInTheWorld = ChunkCoordNativeArray.Length - 1,
            tileDataNativeArray = TileDataNativeArray
        };
        _jobHandleCreateTileData = createTileDataJob.Schedule(_jobHandleCreateChunkCoord);
    }

    private void LateUpdate()
    {
        if (_jobHandleCreateChunkCoord.IsCompleted && !_createChunkCoordsOnce)
        {
            _createChunkCoordsOnce = true;
            _jobHandleCreateChunkCoord.Complete();
        }

        if (_jobHandleCreateTileCoord.IsCompleted && !_createTileCoordsOnce)
        {
            _createTileCoordsOnce = true;
            _jobHandleCreateTileCoord.Complete();
            //foreach (var tileCoord in TileCoordNativeArray)
            //{
            //    Debug.Log($"Tile: " +
            //        $"XCoord {tileCoord.XCoord} / " +
            //        $"YCoord {tileCoord.YCoord}");
            //}
        }

        if (_jobHandleCreateTileData.IsCompleted && !_createTileDataOnce)
        {
            _createTileDataOnce = true;
            _jobHandleCreateTileData.Complete();
            //foreach (var tileData in TileDataNativeArray)
            //{
            //    Debug.Log($"Tile: {tileData.Type}");
            //}
        }
    }

    private void OnApplicationQuit()
    {
        ChunkCoordNativeArray.Dispose();
        TileCoordNativeArray.Dispose();
        TileDataNativeArray.Dispose();
    }
}
