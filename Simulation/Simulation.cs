using FallingSanity.Core;
using FallingSanity.Rendering;
using FallingSanity.Settings;
using FallingSanity.Util;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace FallingSanity.Simulation
{
    /// <summary>
    /// Layer 1 of the material sim: generic movement driven purely by each
    /// cell's MaterialBehavior + Density. No per-material code here — Sand,
    /// Dirt, Coal all fall through the exact same Powder case just by having
    /// Behavior = Powder in their MaterialDefinition.
    ///
    /// Deliberately NOT handled here (future layers, see conversation):
    ///  - Reactions between materials (fire spreading, acid dissolving)
    ///  - Per-cell status flags (Burning, Wet, Bonded)
    ///  - Bespoke one-off behaviors for weird materials
    /// Those get bolted on as additional passes/hooks without touching this
    /// movement logic.
    /// </summary>
    public class Simulation
    {
        private readonly Grid _grid;
        private readonly ChunkManager _chunkManager;
        private readonly WorldRenderer _renderer;
        private readonly Random _rng = new Random();

        // alternated each tick
        private bool _sweepLeftToRight;

        public Simulation(Grid grid, ChunkManager chunkManager, WorldRenderer renderer)
        {
            _grid = grid;
            _chunkManager = chunkManager;
            _renderer = renderer;
        }

        public bool IsPaused { get; set; }
        private bool _stepRequested;

        public void RequestStep() => _stepRequested = true;

        public void Tick()
        {
            if (IsPaused && !_stepRequested) return;
            Step();
            _stepRequested = false;
        }


        /// <summary>
        /// advance the simulation
        /// </summary>
        public void Step()
        {

            Chunk[] chunks = _chunkManager.GetChunks();

            for (int i = 0; i < chunks.Length; i++)
            {
                Point chunkPos = ChunkManager.ChunkPosFromIndex(i);

                //compute in a checkerboard pattern?
                //if (((chunkPos.X + chunkPos.Y) % 2) != parity) continue;

                if (chunks[i].IsActive)
                {
                    Point chunkCellPos = ChunkManager.ChunkPosToCellPos(chunkPos.X, chunkPos.Y);

                    // rows (Y) bottom-to-top so a cell that falls into an
                    // already-processed row isn't moved again this tick
                    for (int row = WorldSettings.DefaultWorldChunkSize - 1; row >= 0; row--)
                    {
                        for (int col = 0; col < WorldSettings.DefaultWorldChunkSize; col++)
                        {
                            StepCell(chunkCellPos.X + col, chunkCellPos.Y + row);
                        }
                    }

                    if (chunks[i].ActiveNextFrame)
                    {
                        chunks[i].ActiveNextFrame = false;
                    }
                    else
                    {
                        chunks[i].IsActive = false;
                    }
                }
                else if (chunks[i].ActiveNextFrame)
                {
                    chunks[i].IsActive = true;
                    chunks[i].ActiveNextFrame = false;
                }
            }
        }

        private void StepCell(int x, int y)
        {
            var cell = _grid.Get(x, y);
            if (cell.MaterialId == MaterialType.Empty) return;

            var matdef = MaterialDatabase.Get(cell.MaterialId);

            Point startpos = new Point(x, y);
            Point currentpos = startpos;

            switch (matdef.Behavior)
            {
                case MaterialBehavior.StaticGas:
                    return; // no forces for atmosphere (background) element

                case MaterialBehavior.Powder:
                case MaterialBehavior.Dust:
                    CalcGravity(currentpos, x, y, cell, matdef);
                    //dest = CalcFriction();
                    break;

                case MaterialBehavior.Liquid:
                    CalcGravity(currentpos, x, y, cell, matdef);
                    //dest = CalcPressure();
                    //dest = CalcFriction();
                    break;

                case MaterialBehavior.Gas:
                    //dest = CalcBuoyancy();
                    break;

                case MaterialBehavior.Solid:
                    return; 
            }

            if (currentpos != startpos)
            {
                //TryMovePos(x, y, currentpos.X, currentpos.Y, out _, out _);
            }
        }

        /// <summary>
        /// Calculates gravity for this movement.
        /// </summary>
        /// <param name="start">the initial position in this tick step</param>
        /// <param name="currentX">the current X position of the calculated pixel (element instance)</param>
        /// <param name="currentY">the current Y position of the calculated pixel (element instance)</param>
        /// <param name="cell">cell ref of the moved pixel (element instance)</param>
        /// <param name="matdef">materialdefinition ref of the moved pixel (element instance)</param>
        private void CalcGravity(Point start, int currentX, int currentY, Cell cell, MaterialDefinition matdef)
        {
            float positionalGravityX = _chunkManager.GetChunkGravityXFromCellPos(start.X, start.Y);
            float positionalGravityY = _chunkManager.GetChunkGravityYFromCellPos(start.X, start.Y);

            float velocityInfluenceX = positionalGravityX * matdef.Weight * matdef.GravityInfluence;

            //ex. -9.81f * 0.017 * 1.0 = -0.16677f = velocity will be set to: currentvelocity + (Abs(-0.16677f) * currentvelocity)
            float velocityInfluenceY = positionalGravityY * matdef.Weight * matdef.GravityInfluence;

            //return new Point(currentX + (int)Math.Round(velocityInfluenceX), currentY + (int)Math.Round(velocityInfluenceY));
            //set the velocity of the modified cell instead of setting position directly, the velocity pass will calculate total velocity

            cell.Velocity = new Vector2(cell.Velocity.X + velocityInfluenceX, cell.Velocity.Y + velocityInfluenceY);
        }

        private Point CalcVelocity(Point start, int currentX, int currentY, Cell cell, MaterialDefinition matdef)
        {
            return new Point(0, 0); // TODO
        }

        // placeholder step fucntions        
        private void StepPowder(int x, int y)
        {
            //try to move downwards first
            if (TryMoveDirect(x, y, x, y + 1)) return;

            int firstDx = _rng.Next(2) == 0 ? -1 : 1;
            if (TryMoveDirect(x, y, x + firstDx, y + 1)) return;
            TryMoveDirect(x, y, x - firstDx, y + 1);
        }

        private void StepLiquid(int x, int y)
        {
            if (TryMoveDirect(x, y, x, y + 1)) return;

            int firstDx = _rng.Next(2) == 0 ? -1 : 1;
            if (TryMoveDirect(x, y, x + firstDx, y + 1)) return;
            if (TryMoveDirect(x, y, x - firstDx, y + 1)) return;

            // Can't fall further — spread sideways looking for the furthest
            // open spot. This is a simplification of real fluid flow (no
            // pressure/volume tracking) but gets most of the visual result.
            const int flowDistance = 4;
            if (TryFlowX(x, y, firstDx, flowDistance)) return;
            TryFlowX(x, y, -firstDx, flowDistance);
        }

        private void StepGas(int x, int y)
        {
            if (TryMoveDirect(x, y, x, y - 1)) return;

            int firstDx = _rng.Next(2) == 0 ? -1 : 1;
            if (TryMoveDirect(x, y, x + firstDx, y - 1)) return;
            TryMoveDirect(x, y, x - firstDx, y - 1);
        }

        private void WakeChunks(int fromX, int fromY, int toX, int toY)
        {
            foreach (var p in _chunkManager.GetAffectedChunkPositionArrayForCellPos(fromX, fromY))
                _chunkManager.MarkChunkDirtyDirectChunkPos(p.X, p.Y);

            foreach (var p in _chunkManager.GetAffectedChunkPositionArrayForCellPos(toX, toY))
                _chunkManager.MarkChunkDirtyDirectChunkPos(p.X, p.Y);
        }

        // placeholder flow function
        private bool TryFlowX(int x, int y, int dx, int maxDistance)
        {
            int targetX = x;
            for (int step = 1; step <= maxDistance; step++)
            {
                int candidateX = x + dx * step;
                if (!_grid.InBounds(candidateX, y)) break;
                if (_grid.Get(candidateX, y).MaterialId != MaterialType.Empty) break;
                targetX = candidateX;
            }

            return targetX != x && TryMoveDirect(x, y, targetX, y);
        }

        /// <summary>
        /// tries to move a cell from one position to another. checks only the target position for avaibility.
        /// </summary>
        /// <param name="fromX">X coordinates of the cell to be moved</param>
        /// <param name="fromY">Y coordinates of the cell to be moved</param>
        /// <param name="toX">X coordinates of the end target position cell</param>
        /// <param name="toY">Y coordinates of the end target position cell</param>
        /// <param name="test">if true, the move will be tested but not executed</param>
        /// <returns>
        /// <para><c>true</c> if the target cell is not occupied and the move has succeeded.</para>
        /// <para><c>false</c> if the target cell is currently occupied and the move has failed.</para>
        /// </returns>
        private bool TryMoveDirect(int fromX, int fromY, int toX, int toY, bool test = false)
        {
            if (!_grid.InBounds(toX, toY)) return false;

            var mover = _grid.Get(fromX, fromY);
            var target = _grid.Get(toX, toY);

            bool canDisplace = target.MaterialId == MaterialType.Empty ||
                MaterialDatabase.Get(target.MaterialId).Density < MaterialDatabase.Get(mover.MaterialId).Density;

            if (!canDisplace) return false;

            if(!test) // execute the move
            {
                _grid.Set(fromX, fromY, target);
                _grid.Set(toX, toY, mover);

                _renderer.UpdatePixel(fromX, fromY, target);
                _renderer.UpdatePixel(toX, toY, mover);

                WakeChunks(fromX, fromY, toX, toY);
            } // if test, we only called the function for the output boolean

            return true;
        }

        /// <summary>
        /// tries to move a cell from one position to another. checks along the path between the starting position and destination pos using DDA
        /// </summary>
        /// <param name="fromX">X coordinates of the cell to be moved</param>
        /// <param name="fromY">Y coordinates of the cell to be moved</param>
        /// <param name="toX">X coordinates of the end target position cell</param>
        /// <param name="toY">Y coordinates of the end target position cell</param>
        /// <param name="endX">the furthest unoccupied X position the pixel has ended its traversal at</param>
        /// <param name="endY">the furthest unoccupied Y position the pixel has ended its traversal at</param>
        /// <returns>
        /// <para><c>true</c> if the target cell is not occupied and the move has succeeded.</para>
        /// <para><c>false</c> if the target cell is currently occupied and the move has failed.</para>
        /// </returns>
        private bool TryMoveToPosition(int fromX, int fromY, int toX, int toY, out int endX, out int endY)
        {
            endX = fromX;
            endY = fromY;

            if (!_grid.InBounds(toX, toY)) return false;

            Point[] path = GridHelper.GetLineTraversalPositionsDDA(fromX, fromY, toX, toY);

            int currentX = fromX;
            int currentY = fromY;

            for (int i = 0; i < path.Length; i++)
            {
                if (!TryMoveDirect(currentX, currentY, path[i].X, path[i].Y, true))
                    return false; // the path is blocked

                currentX = path[i].X;
                currentY = path[i].Y;
                endX = currentX;
                endY = currentY;
            }

            return true; // path end
        }

        
    }
}