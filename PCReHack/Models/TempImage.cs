using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace PCReHack.Models
{
    public class TempImage
    {
        public string Name { get; set; }
        //public int Index { get; set; }
        public Bitmap Bitmap { get; set; }
        public Image<Bgr, byte> Template { get; set; }
        public BitmapSource Image { get; set; }
    }
}