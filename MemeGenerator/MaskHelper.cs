using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace MemeGenerator
{
    public static class MaskHelper
    {
        public static Image<Rgba32> CreateMaskFromImage(Image<Rgba32> image, params PointF[][] drawableAreas)
        {
            return CreateMaskFromImage(image, (IEnumerable<PointF[]>) drawableAreas);
        }

        public static Image<Rgba32> CreateMaskFromImage(Image<Rgba32> image, IEnumerable<PointF[]> drawableAreas)
        {
            var clone = image.Clone();
            clone.Mutate(ctx =>
            {
                foreach (var drawableArea in drawableAreas)
                {
                    ctx.FillPolygon(Rgba32.Transparent, drawableArea);
                }
            });
            return clone;
        }
    }
}
