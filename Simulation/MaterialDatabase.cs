using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FallingSanity.Simulation
{

    public static class MaterialDatabase
    {
        private static readonly Dictionary<MaterialType, MaterialDefinition> _definitions = new();
        private static readonly Random _rng = new Random();

        static MaterialDatabase()
        {
            Register(MaterialType.Empty, new MaterialDefinition
            {
                Name = "Air",
                Behavior = MaterialBehavior.StaticGas,
                ColorPalette = new[] { new Color(12, 12, 16), new Color(12, 12, 14), new Color(12, 12, 12) },
                Density = 0f,
                Toughness = 0f,
                Conductivity = 0f,
                Flammability = 0f,
                MaxHP = 0
            });

            Register(MaterialType.Sand, new MaterialDefinition
            {
                Name = "Sand",
                Behavior = MaterialBehavior.Powder,
                ColorPalette = new[]
                {
                    new Color(237, 201, 175),
                    new Color(230, 193, 165),
                    new Color(224, 185, 155),
                },
                Density = 1.6f,
                Toughness = 0.2f,
                Conductivity = 0.1f,
                Flammability = 0f,
                MaxHP = 10
            });

            Register(MaterialType.Water, new MaterialDefinition
            {
                Name = "Water",
                Behavior = MaterialBehavior.Liquid,
                ColorPalette = new[]
                {
                    new Color(64, 121, 196),
                    new Color(58, 112, 184),
                    new Color(72, 130, 204),
                },
                Density = 1.0f,
                Toughness = 0f,
                Conductivity = 0.6f,
                Flammability = 0f,
                MaxHP = 1
            });
        }

        public static void Register(MaterialType type, MaterialDefinition definition) =>
            _definitions[type] = definition;

        public static MaterialDefinition Get(MaterialType type) => _definitions[type];

        /// <summary>
        /// all registered materials. lets ui enumerate them
        /// </summary>
        public static IEnumerable<MaterialType> AllMaterials => _definitions.Keys;

        /// <summary>
        /// builds a new cell of the given material
        /// </summary>
        public static Cell CreateCell(MaterialType type, byte ambientTemperature = 20)
        {
            var def = Get(type);
            var palette = def.ColorPalette;
            var color = palette != null && palette.Length > 0
                ? palette[_rng.Next(palette.Length)]
                : Color.Magenta; // fallback to the missing texture color

            return new Cell
            {
                MaterialId = type,
                HP = def.MaxHP,
                Color = color,
                Temperature = ambientTemperature
            };
        }
    }
}