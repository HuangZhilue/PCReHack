using System.Globalization;
using System.Windows.Data;

namespace CV_ViewTool.Converters;

public class GreaterThanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return double.TryParse(value?.ToString()??"0", out double intValue)
            && double.TryParse(parameter?.ToString()??"0", out double compareValue)
            ? intValue > compareValue
            : (object)false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}