using MathNet.Numerics.LinearAlgebra;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rem.Commands.MemeGen
{
    static class ImageProcessingContextExtensions
    {
        public static IImageProcessingContext<Rgba32> CreateLayerFromBoundingBox(this IImageProcessingContext<Rgba32> context, BaseBoundingBox boundingBox, string param)
        {
            return context.Apply(img =>
            {
                using (var layer = new Image<Rgba32>(img.Width, img.Height))
                {
                    boundingBox.SetInput(param);
                    layer.Mutate(i => boundingBox.Apply(i));
                    img.Mutate(i => i.DrawImage(layer, 1.0f, new Point(0, 0)));
                }
            });
        }
    }
}
