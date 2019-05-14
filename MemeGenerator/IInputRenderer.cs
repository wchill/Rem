using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace MemeGenerator
{
    public interface IInputRenderer
    {
        bool Render(IImageProcessingContext<Rgba32> context, Rectangle area, object input);
    }
}
