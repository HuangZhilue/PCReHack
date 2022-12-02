using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DeemoHack.Helper;

public static class CommonHelper
{
    public static BitmapSource BitmapToBitmapSource(this Bitmap source)
    {
        try
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        return null;
    }
}
