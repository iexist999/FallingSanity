using System.Numerics;
using FallingSanity.Core;
using FallingSanity.Simulation;
using ImGuiNET;

namespace FallingSanity.UI
{
    /// <summary>
    /// dear-imgui ui
    /// </summary>
    public class MaterialSelectorUI
    {
        private readonly InputHandler _inputHandler;

        public MaterialSelectorUI(InputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }

        /// <summary>call once per frame</summary>
        public void Draw()
        {
            ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.FirstUseEver);
            ImGui.Begin("Materials");

            foreach (var type in MaterialDatabase.AllMaterials)
            {
                if (type == MaterialType.Empty) continue;

                var definition = MaterialDatabase.Get(type);
                bool isSelected = _inputHandler.CurrentMaterial == type;

                if (ImGui.Selectable(definition.Name, isSelected))
                {
                    _inputHandler.SetMaterial(type);
                }
            }

            ImGui.Separator();

            int brushSize = _inputHandler.BrushRadius;
            if (ImGui.SliderInt("Brush size", ref brushSize, InputHandler.MinBrushRadius, InputHandler.MaxBrushRadius))
            {
                _inputHandler.SetBrushRadius(brushSize);
            }

            ImGui.End();
        }
    }
}