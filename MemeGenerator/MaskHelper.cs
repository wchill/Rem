using System;
using System.Collections.Generic;
using System.Linq;
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
        public static Image<Rgba32> CreateMaskFromImage(Image<Rgba32> image, IEnumerable<IReadOnlyList<Point>> drawableAreas)
        {
            var maskLayer = new Image<Rgba32>(image.Width, image.Height);
            maskLayer.Mutate(ctx =>
            {
                ctx.Fill(Rgba32.White);
                using (var cutout = new Image<Rgba32>(maskLayer.Width, maskLayer.Height))
                {
                    cutout.Mutate(maskCtx =>
                    {
                        foreach (var drawableArea in drawableAreas)
                        {
                            // Mark the areas we want to cut out
                            var polygon = drawableArea.Select(p => new PointF(p.X, p.Y)).ToArray();
                            maskCtx.FillPolygon(Rgba32.White, polygon);
                        }
                    });
                    // Now delete those pixels from the actual mask
                    ctx.DrawImage(cutout, PixelBlenderMode.Xor, 1);
                }
            });
            return maskLayer;
        }
    }
}
