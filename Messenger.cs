using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessNutritionTracker
{
    public static class Messenger
    {
        public static event Action? UpdateRequested;

        public static void RequestUpdate() => UpdateRequested?.Invoke();
    }
}
