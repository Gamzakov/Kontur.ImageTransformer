using System.Collections.Generic;
using System.Drawing;
using Kontur.ImageTransformer.Attributes;

// ReSharper disable UnusedMember.Global

namespace Kontur.ImageTransformer
{
    [ContainsFilterProcessors]
    public class ImageProcessors
    {
        [FilterProcessor("RotateCCW")]
        public Bitmap ApplyRotateCCW(Bitmap image, IReadOnlyList<int> coords)
        {
            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            var selection = TakeSelecetionRectangle(image.Width, image.Height, coords);
            
            if (selection == Rectangle.Empty)
                return null;
            
            var bmp = CropBitmap(image, selection);

            return bmp;
        }

        [FilterProcessor("RotateCW")]
        public Bitmap ApplyRotateCW(Bitmap image, IReadOnlyList<int> coords)
        {
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            var selection = TakeSelecetionRectangle(image.Width, image.Height, coords);
            
            if (selection == Rectangle.Empty)
                return null;
            
            var bmp = CropBitmap(image, selection);

            return bmp;
        }

        [FilterProcessor("FlipH")]
        public Bitmap ApplyFlipH(Bitmap image, IReadOnlyList<int> coords)
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            var selection = TakeSelecetionRectangle(image.Width, image.Height, coords);
            
            if (selection == Rectangle.Empty)
                return null;
            
            var bmp = CropBitmap(image, selection);

            return bmp;
        }

        [FilterProcessor("FlipV")]
        public Bitmap ApplyFlipV(Bitmap image, IReadOnlyList<int> coords)
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            var selection = TakeSelecetionRectangle(image.Width, image.Height, coords);
            
            if (selection == Rectangle.Empty)
                return null;
            
            var bmp = CropBitmap(image, selection);

            return bmp;
        }

        private Rectangle TakeSelecetionRectangle(int wBmp, int hBmp, IReadOnlyList<int> coords)
        {
            var x = coords[0];
            var y = coords[1];
            var rW = coords[2];
            var rH = coords[3];

            var selection = new Rectangle(x, y, rW, rH);
            var bmpRectangle = new Rectangle(0, 0, wBmp, hBmp);

            var result = Rectangle.Intersect(bmpRectangle, selection);

            return result;
        }

        private Bitmap CropBitmap(Bitmap bitmap, Rectangle selection)
        {
            if (bitmap == null)
                return null;

            bitmap = bitmap.Clone(selection, bitmap.PixelFormat);
            
            return bitmap;
        }
    }
}