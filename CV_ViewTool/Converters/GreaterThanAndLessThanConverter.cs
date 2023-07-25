using System.Globalization;
using System.Windows.Data;

namespace CV_ViewTool.Converters;

public class GreaterThanAndLessThanConverter : IValueConverter
{
    public static double DefaultCompareValue { get; set; } = double.NaN;
    public static double DefaultToleranceValue { get; set; } = double.NaN;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        parameter ??= DefaultCompareValue;
        DefaultToleranceValue = double.IsNaN(DefaultToleranceValue) ? 0 : DefaultToleranceValue;

        return double.TryParse(value?.ToString()??"0", out double intValue)
            && double.TryParse(parameter?.ToString()??"0", out double compareValue)
            ? (intValue > compareValue - DefaultToleranceValue && intValue < compareValue + DefaultToleranceValue)
            : (object)false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}