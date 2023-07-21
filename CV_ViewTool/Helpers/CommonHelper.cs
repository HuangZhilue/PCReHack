using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CV_ViewTool.Helpers;

public static class CommonHelper
{
    public static BitmapSource BitmapToBitmapSource(this Bitmap source)
    {
        try
        {
            //return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            //              source.GetHbitmap(),
            //              IntPtr.Zero,
            //              Int32Rect.Empty,
            //              BitmapSizeOptions.FromEmptyOptions());
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            source.GetHbitmap(),
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

            // 创建新的 BitmapSource 对象后，释放原始的 HBitmap
            source.Dispose();

            return bitmapSource;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        return null;
    }

    public static BitmapImage BitmapToBitmapImage(this Bitmap bitmap)
    {
        if (bitmap is null) return null;

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
}
