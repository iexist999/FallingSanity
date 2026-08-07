using Microsoft.Xna.Framework;

namespace FallingSanity.Simulation
{
    /// <summary>
    /// Per-pixel instance data
    /// </summary>
    public struct Cell
    {
        public MaterialType MaterialId;  // currently occupying material
        public byte HP;                  // remaining durability. used along with material toughness
        public Color Color;              // very basic rendering
        public float Temperature;        // temperature
        public float Pressure;           // pressure
        public Vector2 Velocity;         // velocity
        private bool IsActive { get; set; }

        // the default atmosphere medium. air by default. make it changeable in the world settings later
        public static readonly Cell Empty = new Cell
        {
            MaterialId = MaterialType.Empty,
            HP = 0,
            Color = new Color(12, 12, 16),
            Temperature = 20f,
        };
    }
}