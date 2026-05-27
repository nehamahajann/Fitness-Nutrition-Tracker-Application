using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FitnessNutritionTracker.Helpers
{
    class Utility
    {
        public static void DebugPrint(object obj)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            string json = JsonSerializer.Serialize(obj, options);

            Console.WriteLine(json);

            System.Diagnostics.Debug.WriteLine(json);
        }


        public static void DebugShowBox(object obj)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            string json = JsonSerializer.Serialize(obj, options);

            MessageBox.Show(json);
        }

    }
}
