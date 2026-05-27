using FitnessNutritionTracker.Helpers;
using FitnessNutritionTracker.ViewModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;


namespace FitnessNutritionTracker
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ActivityLogsPage : UserControl
    {
        public ActivityLogsPage()
        {
            InitializeComponent();

            var viewmodel = new ActivityLogsPageModel();

            this.DataContext = viewmodel;
        }
    }


    [ValueConversion(typeof(long), typeof(string))]
    public class LongToTimeConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long timestamp)
            {
                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(timestamp);

                string? format = parameter as string;

                return dto.LocalDateTime.ToString(format, culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
