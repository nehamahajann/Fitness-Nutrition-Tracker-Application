using FitnessNutritionTracker.Helpers;
using FitnessNutritionTracker.ViewModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;


namespace FitnessNutritionTracker
{
    /// <summary>
    /// 
    /// </summary>
    public partial class HealthSuggestionPage : UserControl
    {
        public HealthSuggestionPage()
        {
            InitializeComponent();

            var viewmodel = new HealthSuggestionPageModel();

            this.DataContext = viewmodel;
        }
    }


    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
