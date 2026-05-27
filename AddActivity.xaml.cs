using FitnessNutritionTracker.Repository;
using FitnessNutritionTracker.ViewModel;
using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace FitnessNutritionTracker {
    public class EnumDescriptionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Binding.DoNothing;

            foreach (var field in targetType.GetFields())
            {
                var attr = field.GetCustomAttribute<DescriptionAttribute>();
                if ((attr != null && (string)value == attr.Description) || field.Name == (string)value)
                    return Enum.Parse(targetType, field.Name);
            }
            return Binding.DoNothing;
        }
    }



    public partial class AddActivity : UserControl
    {
        public AddActivity()
        {
            InitializeComponent();
            
            this.DataContext = new ActivityViewModel();
        }
    }
}