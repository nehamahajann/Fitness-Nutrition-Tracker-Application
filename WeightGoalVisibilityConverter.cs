using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FitnessNutritionTracker
{
    public class WeightGoalVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assuming the Name property is passed in as 'Weight'
            if (value is string goalName && goalName.Equals("Weight", StringComparison.OrdinalIgnoreCase))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
