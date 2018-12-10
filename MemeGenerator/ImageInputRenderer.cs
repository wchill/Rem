using System;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace MemeGenerator
{
    public class ImageInputRenderer : IInputRenderer
    {
        private readonly ImageScalingMode _scalingMode;
        private readonly GraphicsOptions _graphicsOptions;

        public ImageInputRenderer(ImageScalingMode scalingMode, GraphicsOptions graphicsOptions)
        {
            _scalingMode = scalingMode;
            _graphicsOptions = graphicsOptions;
        }

        public bool Render(IImageProcessingContext<Rgba32> context, Rectangle area, object input)
        {
            if (!(input is Lazy<Image<Rgba32>> lazyImage))
            {
                return false;
            }

            Image<Rgba32> image;
            try
            {
                image = lazyImage.Value;
            }
            catch (Exception)
            {
                // Failed to load image (404, or it wasn't an image, etc), let something else take care of it.
                return false;
            }

            switch (_scalingMode)
            {
                case ImageScalingMode.None:
                    ApplyNone(context, area, image);
                    break;
                case ImageScalingMode.Center:
                    ApplyCenter(context, area, image);
                    break;
                case ImageScalingMode.StretchFit:
                    ApplyStretchFit(context, area, image);
                    break;
                case ImageScalingMode.FillFit:
                    ApplyFillFit(context, area, image);
                    break;
                case ImageScalingMode.FitWithLetterbox:
                    ApplyFitWithLetterbox(context, area, image);
                    break;
                case ImageScalingMode.Tile:
                    ApplyTile(context, area, image);
                    break;
                default:
                    throw new ArgumentException("Invalid scaling mode specified.");
            }

            return true;
        }

        private void ApplyNone(IImageProcessingContext<Rgba32> context, Rectangle area, Image<Rgba32> image)
        {
            using (var img2 = image.Clone())
            {
                if (img2.Width > area.Width || img2.Height > area.Height)
                {
                    var newWidth = Math.Min(area.Width, img2.Width);
                    var newHeight = Math.Min(area.Height, img2.Height);
                    img2.Mutate(ctx => ctx.Crop(newWidth, newHeight));
                }
                context.DrawImage(_graphicsOptions, img2, area.Location);
            }
        }

        private void ApplyCenter(IImageProcessingContext<Rgba32> context, Rectangle area, Image<Rgba32> image)
        {
            using (var img2 = image.Clone())
            {
                if (img2.Width > area.Width || img2.Height > area.Height)
                {
                    var newWidth = Math.Min(area.Width, img2.Width);
                    var newHeight = Math.Min(area.Height, img2.Height);
                    var x = Math.Max(0, (image.Width - area.Width) / 2);
                    var y = Math.Max(0, (image.Height - area.Height) / 2);
                    img2.Mutate(ctx => ctx.Crop(new Rectangle(x, y, newWidth, newHeight)));
                }
                var sx = area.X + (area.Width - img2.Width) / 2;
                var sy = area.Y + (area.Height - img2.Height) / 2;

                context.DrawImage(_graphicsOptions, img2, new Point(sx, sy));
            }
        }

        private void ApplyStretchFit(IImageProcessingContext<Rgba32> context, Rectangle area, Image<Rgba32> image)
        {
            using (var img2 = image.Clone())
            {
                img2.Mutate(ctx => ctx.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Stretch,
                    Sampler = area.Width > image.Width ? (IResampler) new BicubicResampler() : new BoxResampler(),
                    Size = new Size(area.Width, area.Height)
                }));

                context.DrawImage(_graphicsOptions, img2, area.Location);
            }
        }
        
        private void ApplyFillFit(IImageProcessingContext<Rgba32> context, Rectangle area, Image<Rgba32> image)
        {
            using (var img2 = image.Clone())
            {
                img2.Mutate(ctx =>
                {
                    var size = area.Width > image.Width ? new Size(area.Width, 0) : new Size(0, area.Height);
                    var resizeOptions = new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Compand = false,
                        Sampler = area.Width > image.Width ? (IResampler) new BicubicResampler() : new BoxResampler(),
                        Size = size
                    };
                    ctx.Resize(resizeOptions);
                });

                if (img2.Width > area.Width || img2.Height > area.Height)
                {
                    var newWidth = Math.Min(area.Width, img2.Width);
                    var newHeight = Math.Min(area.Height, img2.Height);
                    var x = Math.Max(0, (image.Width - area.Width) / 2);
                    var y = Math.Max(0, (image.Height - area.Height) / 2);
                    img2.Mutate(ctx => ctx.Crop(new Rectangle(x, y, newWidth, newHeight)));
                }

                context.DrawImage(_graphicsOptions, img2, area.Location);
            }
        }

        private void ApplyFitWithLetterbox(IImageProcessingContext<Rgba32> context, Rectangle area, Image<Rgba32> image)
        {
            using (var img2 = image.Clone())
            {
                img2.Mutate(ctx =>
                {
                    var resizeOptions = new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Compand = false,
                        Sampler = area.Width > image.Width ? (IResampler)new BicubicResampler() : new BoxResampler(),
                        Size = area.Size
                    };
                    ctx.Resize(resizeOptions);
                });

                var sx = area.X + (area.Width - img2.Width) / 2;
                var sy = area.Y + (area.Height - img2.Height) / 2;

                context.DrawImage(_graphicsOptions, img2, new Point(sx, sy));
            }
        }

        private void ApplyTile(IImageProcessingContext<Rgba32> context, Rectangle area, Image<Rgba32> image)
        {
            throw new NotImplementedException("Tile fit has not yet been implemented.");
        }
    }
}