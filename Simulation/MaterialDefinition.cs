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
        public Color[] ColorPalette; // a few shade variants so cells aren't flat-colored

        public float Density; // (kg/m^3) determines sinking rules
        public float Weight; // (kg) weight affects gravity influence
        public float Toughness;
        public float Hardness;
        public float Conductivity;
        public float Flammability;

        /// <summary>
        /// indicates how much kinetic energy is retained after collision.
        /// 1.0e - no energy is preserved. 100% of the initial velocity gets absorbed by the hit pixel
        /// 0.0e - energy is fully preserved, splitting the total velocity between the two objects, as they continue moving
        /// </summary>
        public float RestitutionCoefficient; // (0.0e - 1.0e) collision elasticity

        public float GravityInfluence;

        public byte MaxHP;
    }
}