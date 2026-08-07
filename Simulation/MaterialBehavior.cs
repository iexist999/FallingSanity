namespace FallingSanity.Simulation
{

    public enum MaterialBehavior : byte
    {
        Solid,   // never moves on its own: stone, glass, metal
        Powder,  // falls straight down / diagonally: sand, dust
        Liquid,  // falls and spreads sideways: water, oil, lava
        Gas,     // rises and disperses: steam, smoke
        Dust,    // like Powder but lighter, disperses more: ash, spores

        StaticGas// the default atmosphere medium. spreads into empty neighbor cells
    }
}