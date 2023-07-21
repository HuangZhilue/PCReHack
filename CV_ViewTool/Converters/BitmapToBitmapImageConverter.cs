using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CV_ViewTool.Converters;

public class BitmapToBitmapImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Bitmap bitmap || bitmap is null) return null;

        using MemoryStream memoryStream = new();
        // 将 Bitmap 对象保存到 MemoryStream
        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
        memoryStream.Position = 0;

        // 创建新的 BitmapImage 对象并设置源
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memoryStream;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze(); // 冻结 BitmapImage，以便在多个线程或异步操作中使用

        return bitmapImage;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
