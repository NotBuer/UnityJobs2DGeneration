using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using Unity.VisualScripting;
using System.Threading;

public struct ChunkData
{
    public ChunkCoord ChunkCoord { get; set; }
    public ConcurrentDictionary<TileCoord, TileData> ChunkDataDictionary { get; set; }

    public ChunkData(ChunkCoord chunkCoord, ConcurrentDictionary<TileCoord, TileData> chunkDataDictionary)
    {
        ChunkCoord = chunkCoord;
        ChunkDataDictionary = chunkDataDictionary;
    }

    //public async Task ParallelCreateChunkData(CancellationTokenSource tokenSource)
    //{
    //    ConcurrentDictionary<TileCoord, TileData> chunkData =
    //            new(Environment.ProcessorCount, Chunk.TOTAL_SIZE);

    //    await Task.Run(() =>
    //    {
    //        for (byte y = 0; y < Chunk.Y_SIZE; y++)
    //        {
    //            for (byte x = 0; x < Chunk.X_SIZE; x++)
    //            {
    //                chunkData.TryAdd(new TileCoord(x, y), new TileData(Tile.TileType.Dirt));
    //            }
    //        }
    //    }, tokenSource.Token);

    //    ChunkDataDictionary.AddRange(chunkData.ToArray());
    //}
}
