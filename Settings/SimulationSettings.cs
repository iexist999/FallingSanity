using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallingSanity.Settings
{
    public static class SimulationSettings
    {
        // simulation quality

        /// <summary>
        /// Determines whether friction should be calculated for all cells in the traversal path
        /// of a moving pixel. If false, only the neighbouring pixels in the initial frame position
        /// will be considered for friction calculation.
        /// </summary>
        public static bool CalculateFrictionForAllInTraversal { get; set; } = true;
    }
}
