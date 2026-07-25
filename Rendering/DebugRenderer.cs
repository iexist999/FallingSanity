using FallingSanity.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallingSanity.Rendering
{
    public class DebugRenderer
    {
        private readonly Texture2D _pixel;

        private static readonly Color ActiveColor = new Color(0, 255, 0, 140);
        private static readonly Color InactiveColor = new Color(255, 0, 0, 140);
        private static readonly Color ActiveNextFrameColor = new Color(255, 255, 0, 140);

        public DebugRenderer(GraphicsDevice device)
        {
            _pixel = new Texture2D(device, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(SpriteBatch spriteBatch, ChunkManager chunkManager, int chunkSizeCells, int cellSize, int thickness = 2)
        {
            Chunk[] chunks = chunkManager.GetChunks();

            for (int i = 0; i < chunks.Length; i++)
            {
                Point chunkPos = ChunkManager.ChunkPosFromIndex(i);
                Point cellPos = ChunkManager.ChunkPosToCellPos(chunkPos.X, chunkPos.Y);

                var rect = new Rectangle(
                    cellPos.X * cellSize,
                    cellPos.Y * cellSize,
                    chunkSizeCells * cellSize,
                    chunkSizeCells * cellSize);

                Color color = chunks[i].IsActive
                    ? ActiveColor
                    : chunks[i].ActiveNextFrame
                        ? ActiveNextFrameColor
                        : InactiveColor;

                DrawRectOutline(spriteBatch, rect, color, thickness);
            }
        }

        private void DrawRectOutline(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}
