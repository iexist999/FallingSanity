using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallingSanity.UI
{
    public class DebugToolsUI
    {
        private readonly Simulation.Simulation _simulation;

        public bool DrawChunks { get; private set; }

        public DebugToolsUI(Simulation.Simulation simulation)
        {
            _simulation = simulation;
        }

        public void Draw()
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(10, 140), ImGuiCond.FirstUseEver);
            ImGui.Begin("Debug");

            bool isPaused = _simulation.IsPaused;
            if (ImGui.Checkbox("Paused", ref isPaused))
                _simulation.IsPaused = isPaused;

            ImGui.BeginDisabled(!isPaused);
            if (ImGui.Button("Step"))
                _simulation.RequestStep();
            ImGui.EndDisabled();

            ImGui.Separator();

            bool drawChunks = DrawChunks;
            if (ImGui.Checkbox("Draw chunks", ref drawChunks))
                DrawChunks = drawChunks;

            ImGui.TextDisabled("green = active");
            ImGui.TextDisabled("red = inactive");

            ImGui.End();
        }
    }
}
