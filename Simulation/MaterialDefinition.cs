using Microsoft.Xna.Framework;

namespace FallingSanity.Simulation
{
    /// <summary>
    /// per-material static data
    /// </summary>
    public struct MaterialDefinition
    {
        public string Name;
        public MaterialBehavior Behavior;
        public Color[] ColorPalette;   // a few shade variants so cells aren't flat-colored

        public float Density;          // (kg/m^3) determines sinking rules
        public float Weight;           // (kg) weight affects gravity influence
        public float Toughness;
        public float Hardness;
        public float Conductivity;
        public float Flammability;

        public float GravityInfluence;

        public byte MaxHP;
    }
}