using System.Drawing;

namespace TIM.Devices.Framework.Common.Extensions
{
    public static class BitmapExtensions
    {
        public static Rectangle GetBounds(this Bitmap bmpBitmap)
        {
            return new Rectangle(0, 0, bmpBitmap.Width, bmpBitmap.Height);
        }
    }
}