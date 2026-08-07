using FallingSanity.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FallingSanity.Simulation
{
    public class ChunkManager
    {
        private readonly Grid _grid;
        private Chunk[] _chunks;
        public Chunk[] GetChunks() => _chunks;
        private static int _chunkSize;

        // horizontal and vertical chunk count
        private static int _chunksHorizontal, _chunksVertical;

        public ChunkManager(Grid grid, int gridWidth, int gridHeight, int chunkSize)
        {
            _grid = grid;
            _chunkSize = chunkSize;
            // calculate the total amount of chunks vertically and horizonstally
            // based on height and width of the world grid
            _chunksHorizontal = (gridWidth + chunkSize - 1) / chunkSize;
            _chunksVertical = (gridHeight + chunkSize - 1) / chunkSize; //round up
            _chunks = new Chunk[_chunksHorizontal * _chunksVertical];

            for (int i = 0; i < _chunks.Length; i++)
            {
                _chunks[i] = new Chunk(ChunkPosFromIndex(i).X, ChunkPosFromIndex(i).Y);
                _chunks[i].IsActive = true;
            }
        }

        // util

        public bool InBoundsCellPos(int cellX, int cellY)
        {
            int index = ChunkIndexFromCellPos(cellX, cellY);
            return _chunks.Length > index && index >= 0;
        }

        public bool InBoundsChunkPos(int chunkX, int chunkY)
        {
            int index = ChunkIndexFromChunkPos(chunkX, chunkY);
            return _chunks.Length > index && index >= 0;
        }

        public static int ChunkIndexFromCellPos(int cellX, int cellY) =>
            (cellY / _chunkSize) * _chunksHorizontal + (cellX / _chunkSize);

        public static int ChunkIndexFromChunkPos(int chunkX, int chunkY) =>
            (chunkY * _chunksHorizontal) + chunkX;

        public static Point ChunkPosFromIndex(int index)
        {
            int x = index % _chunksHorizontal;
            int y = index / _chunksHorizontal;
            return new Point(x, y);
        }

        public static Point CellPosToChunkPos(int cellX, int cellY) =>
            new Point((cellX / _chunkSize), (cellY / _chunkSize));

        public static Point ChunkPosToCellPos(int chunkX, int chunkY) =>
            new Point((chunkX * _chunkSize), (chunkY * _chunkSize));

        public static List<Point> GetAffectedChunkPositionListForCellPos(int cellX, int cellY)
        {
            var result = new List<Point>();

            Point chunkPos = CellPosToChunkPos(cellX, cellY);
            int localX = cellX - chunkPos.X * _chunkSize;
            int localY = cellY - chunkPos.Y * _chunkSize;

            int edgeDx = localX == 0 ? -1 : (localX == _chunkSize - 1 ? 1 : 0);
            int edgeDy = localY == 0 ? -1 : (localY == _chunkSize - 1 ? 1 : 0);

            int[] xOffsets = edgeDx == 0 ? new[] { 0 } : new[] { 0, edgeDx };
            int[] yOffsets = edgeDy == 0 ? new[] { 0 } : new[] { 0, edgeDy };

            foreach (int oy in yOffsets)
            {
                foreach (int ox in xOffsets)
                {
                    int nx = chunkPos.X + ox;
                    int ny = chunkPos.Y + oy;

                    if (nx < 0 || ny < 0 || nx >= _chunksHorizontal || ny >= _chunksVertical) continue;

                    result.Add(new Point(nx, ny));
                }
            }

            return result;
        }

        public Point[] GetAffectedChunkPositionArrayForCellPos(int cellX, int cellY)
        {
            Point chunkPos = CellPosToChunkPos(cellX, cellY);
            int localX = cellX - chunkPos.X * _chunkSize;
            int localY = cellY - chunkPos.Y * _chunkSize;

            int edgeDx = localX == 0 ? -1 : (localX == _chunkSize - 1 ? 1 : 0);
            int edgeDy = localY == 0 ? -1 : (localY == _chunkSize - 1 ? 1 : 0);

            int[] xOffsets = edgeDx == 0 ? new[] { 0 } : new[] { 0, edgeDx };
            int[] yOffsets = edgeDy == 0 ? new[] { 0 } : new[] { 0, edgeDy };

            
            Span<Point> candidates = stackalloc Point[4];
            int count = 0;

            foreach (int oy in yOffsets)
            {
                foreach (int ox in xOffsets)
                {
                    int nx = chunkPos.X + ox;
                    int ny = chunkPos.Y + oy;

                    if (nx < 0 || ny < 0 || nx >= _chunksHorizontal || ny >= _chunksVertical) continue;

                    candidates[count++] = new Point(nx, ny);
                }
            }

            var result = new Point[count];
            for (int i = 0; i < count; i++) result[i] = candidates[i];
            return result;
        }

        public Chunk ChunkFromChunkPos(int chunkX, int chunkY)
        {
            return _chunks[ChunkIndexFromChunkPos(chunkX, chunkY)];
        }

        public Chunk ChunkFromCellPos(int cellX, int cellY)
        {
            return _chunks[ChunkIndexFromCellPos(cellX, cellY)];
        }

        public float GetChunkGravityXFromChunkPos(int chunkX, int chunkY) =>
            _chunks[ChunkIndexFromChunkPos(chunkX, chunkY)].GravityX;
        public float GetChunkGravityYFromChunkPos(int chunkX, int chunkY) =>
            _chunks[ChunkIndexFromChunkPos(chunkX, chunkY)].GravityY;

        public float GetChunkGravityXFromCellPos(int cellX, int cellY) =>
            _chunks[ChunkIndexFromCellPos(cellX, cellY)].GravityX;
        public float GetChunkGravityYFromCellPos(int cellX, int cellY) =>
            _chunks[ChunkIndexFromCellPos(cellX, cellY)].GravityY;

        public bool IsChunkActiveCellPos(int cellX, int cellY) => 
            _chunks[ChunkIndexFromCellPos(cellX, cellY)].IsActive ? true : false;

        public bool IsChunkActiveChunkPos(int chunkX, int chunkY) =>
            _chunks[ChunkIndexFromChunkPos(chunkX, chunkY)].IsActive ? true : false;

        public void MarkAllNeighborsDirtyCellPos(int cellX, int cellY)
        {
            int cx = cellX / _chunkSize, cy = cellY / _chunkSize;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = cx + dx, ny = cy + dy;
                    if (nx < 0 || ny < 0 || nx >= _chunksHorizontal || ny >= _chunksVertical) continue;
                    _chunks[ny * _chunksHorizontal + nx].ActiveNextFrame = true;
                }
            }
        }

        public void MarkAllNeighborsDirtyChunkPos(int chunkX, int chunkY)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = chunkX + dx, ny = chunkY + dy;
                    if (nx < 0 || ny < 0 || nx >= _chunksHorizontal || ny >= _chunksVertical) continue;
                    _chunks[ny * _chunksHorizontal + nx].ActiveNextFrame = true;
                }
            }
        }

        public void MarkChunkDirtyDirectCellPos(int cellX, int cellY)
        {
            if (!_grid.InBounds(cellX, cellY)) return;
            _chunks[ChunkIndexFromCellPos(cellX, cellY)].ActiveNextFrame = true;
        }

        public void MarkChunkDirtyDirectChunkPos(int chunkX, int chunkY)
        {
            if (!InBoundsChunkPos(chunkX, chunkY)) return;
            _chunks[ChunkIndexFromChunkPos(chunkX, chunkY)].ActiveNextFrame = true;
        }

        public void DeactivateChunkFromCellPos(int cellX, int cellY)
        {
            if (!_grid.InBounds(cellX, cellY)) return;
            _chunks[ChunkIndexFromCellPos(cellX, cellY)].IsActive = false;
        }

        public void DeactivateChunkFromChunkPos(int chunkX, int chunkY)
        {
            if (!InBoundsChunkPos(chunkX, chunkY)) return;
            _chunks[ChunkIndexFromChunkPos(chunkX, chunkY)].IsActive = false;
        }
    }
}
