using ColorPicker.Models;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace CV_ViewTool.Converters;

public class ColorStateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ColorState colorState)
        {
            return HSVToHexWithAlpha(colorState.HSV_H, colorState.HSV_S, colorState.HSV_V, 255);
        }

        return "#00000000";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString)
        {
            Color color = (Color)(new ColorConverter().ConvertFromString(colorString));

            // 转换为RGB表示
            byte red = color.R;
            byte green = color.G;
            byte blue = color.B;
            byte alpha = color.A;

            // 转换为HSV表示
            double h, s, v;
            RGBToHSV(red, green, blue, out h, out s, out v);
            double h1, s1, l;
            RGBToHSL(red, green, blue, out h1, out s1, out l);

            return new ColorState(red, green, blue, alpha, h, s, v, h1, s1, l);
        }

        return null;
    }

    static void RGBToHSL(byte r, byte g, byte b, out double h, out double s, out double l)
    {
        double red = r / 255.0;
        double green = g / 255.0;
        double blue = b / 255.0;

        double max = Math.Max(red, Math.Max(green, blue));
        double min = Math.Min(red, Math.Min(green, blue));

        // 计算亮度（Lightness）
        l = (max + min) / 2;

        double delta = max - min;
        // 计算饱和度（Saturation）
        if (max == min)
        {
            s = 0; // 灰色，饱和度为0
        }
        else
        {
            s = delta / (1 - Math.Abs(2 * l - 1));
        }

        // 计算色相（Hue）
        double hue;
        if (max == min)
        {
            hue = 0; // 无色，色相为0
        }
        else if (max == red)
        {
            hue = (60 * ((green - blue) / delta) + 360) % 360;
        }
        else if (max == green)
        {
            hue = (60 * ((blue - red) / delta) + 120) % 360;
        }
        else
        {
            hue = (60 * ((red - green) / delta) + 240) % 360;
        }

        h = hue;
    }

    static void RGBToHSV(byte r, byte g, byte b, out double h, out double s, out double v)
    {
        double red = r / 255.0;
        double green = g / 255.0;
        double blue = b / 255.0;

        double max = Math.Max(red, Math.Max(green, blue));
        double min = Math.Min(red, Math.Min(green, blue));

        double hue = 0;
        if (max == min)
        {
            hue = 0; // 无色，色相为0
        }
        else if (max == red)
        {
            hue = (60 * ((green - blue) / (max - min)) + 360) % 360;
        }
        else if (max == green)
        {
            hue = (60 * ((blue - red) / (max - min)) + 120) % 360;
        }
        else if (max == blue)
        {
            hue = (60 * ((red - green) / (max - min)) + 240) % 360;
        }

        double saturation = (max == 0) ? 0 : (1 - (min / max));
        double value = max;

        h = hue;
        s = saturation;
        v = value;
    }
    static string HSVToHexWithAlpha(double h, double s, double v, byte alpha)
    {
        int hi = System.Convert.ToInt32(Math.Floor(h / 60)) % 6;
        double f = (h / 60) - Math.Floor(h / 60);

        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);

        byte red, green, blue;

        if (hi == 0)
        {
            red = System.Convert.ToByte(v * 255);
            green = System.Convert.ToByte(t * 255);
            blue = System.Convert.ToByte(p * 255);
        }
        else if (hi == 1)
        {
            red = System.Convert.ToByte(q * 255);
            green = System.Convert.ToByte(v * 255);
            blue = System.Convert.ToByte(p * 255);
        }
        else if (hi == 2)
        {
            red = System.Convert.ToByte(p * 255);
            green = System.Convert.ToByte(v * 255);
            blue = System.Convert.ToByte(t * 255);
        }
        else if (hi == 3)
        {
            red = System.Convert.ToByte(p * 255);
            green = System.Convert.ToByte(q * 255);
            blue = System.Convert.ToByte(v * 255);
        }
        else if (hi == 4)
        {
            red = System.Convert.ToByte(t * 255);
            green = System.Convert.ToByte(p * 255);
            blue = System.Convert.ToByte(v * 255);
        }
        else
        {
            red = System.Convert.ToByte(v * 255);
            green = System.Convert.ToByte(p * 255);
            blue = System.Convert.ToByte(q * 255);
        }

        return $"#{alpha:X2}{red:X2}{green:X2}{blue:X2}";
    }
}