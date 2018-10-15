using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using RestSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Rem.Commands.MemeGen
{
    public enum ImageScalingMode
    {
        None,
        FillFit,
        FitWithLetterbox,
        StretchFit,
        Tile,
        Center
    }

    public class ImageBoundingBox : BaseBoundingBox
    {
        private static readonly LRUCache<string, Image<Rgba32>> ImageCache = new LRUCache<string, Image<Rgba32>>(10);
        private static readonly LRUCache<string, bool> IsImageCache = new LRUCache<string, bool>(1000);

        private static readonly HashSet<string> ValidMimeTypes = new HashSet<string>
        {
            "image/bmp",
            "image/gif",
            "image/jpeg",
            "image/png",
            // "image/webp"
        };

        public GraphicsOptions GraphicsOptions { get; set; } = GraphicsOptions.Default;
        public ImageScalingMode ScalingMode { get; set; } = ImageScalingMode.FitWithLetterbox;

        public ImageBoundingBox()
        {

        }

        public ImageBoundingBox(float x, float y, float w, float h)
        {
            TopLeft = new PointF(x, y);
            TopRight = new PointF(x + w, y);
            BottomLeft = new PointF(x, y + h);
            BottomRight = new PointF(x + w, y + h);
        }

        public override async Task<bool> CanHandleAsync(string input)
        {
            var inIsImageCache = IsImageCache.TryGet(input, out bool isImage);
            if (inIsImageCache)
            {
                return isImage;
            }

            var result = Uri.TryCreate(input, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                return false;
            }

            var client = new RestClient($"{uriResult.Scheme}://{uriResult.Authority}");
            var request = new RestRequest(uriResult.AbsolutePath)
            {
                ResponseWriter = (responseStream) => { }
            };
            var response = await client.ExecuteGetTaskAsync(request);

            isImage = ValidMimeTypes.Contains(response.ContentType);
            IsImageCache.Add(input, isImage);
            return isImage;
        }

        internal override async Task<Matrix<float>> ApplyAsyncInternal(IImageProcessingContext<Rgba32> context)
        {
            var image = await GetImageFromUrl(_lastInput);
            switch (ScalingMode)
            {
                case ImageScalingMode.None:
                    return ApplyNone(context, image);
                case ImageScalingMode.FillFit:
                    return ApplyFillFit(context, image);
                case ImageScalingMode.FitWithLetterbox:
                    return ApplyFitWithLetterbox(context, image);
                case ImageScalingMode.StretchFit:
                    return ApplyStretchFit(context, image);
                case ImageScalingMode.Tile:
                    return ApplyTile(context, image);
                case ImageScalingMode.Center:
                    return ApplyCenter(context, image);
                default:
                    return ApplyNone(context, image);
            }
        }

        private Matrix<float> ApplyNone(IImageProcessingContext<Rgba32> context, Image<Rgba32> image)
        {
            context.DrawImage(GraphicsOptions, image, new Point(0, 0));
            return GetProjectiveTransformationMatrix();
        }

        private Matrix<float> ApplyFillFit(IImageProcessingContext<Rgba32> context, Image<Rgba32> image)
        {
            var imageAspectRatio = image.Width / (double)image.Height;
            var boxAspectRatio = MaxWidth / (double)MaxHeight;
            double scaledWidth = image.Width;
            double scaledHeight = image.Height;
            Point drawingPoint = new Point(0, 0);
            if (imageAspectRatio < boxAspectRatio)
            {
                // box is wider than image
                scaledHeight = image.Width / boxAspectRatio;
                scaledWidth = image.Width;
                drawingPoint = new Point(0, (int)((scaledHeight - image.Height) / 2));
            }
            else if (boxAspectRatio < imageAspectRatio)
            {
                // image is wider than box
                scaledWidth = image.Height * boxAspectRatio;
                scaledHeight = image.Height;
                drawingPoint = new Point((int)((scaledWidth - image.Width) / 2), 0);
            }
            context.DrawImage(GraphicsOptions, image, drawingPoint);
            return GetProjectiveTransformationMatrix((float)scaledWidth, (float)scaledHeight);
        }

        private Matrix<float> ApplyFitWithLetterbox(IImageProcessingContext<Rgba32> context, Image<Rgba32> image)
        {
            var imageAspectRatio = image.Width / (double)image.Height;
            var boxAspectRatio = MaxWidth / (double)MaxHeight;
            double scaledWidth = image.Width;
            double scaledHeight = image.Height;
            Point drawingPoint = new Point(0, 0);
            if (imageAspectRatio < boxAspectRatio)
            {
                // box is wider than image
                scaledWidth = image.Height * boxAspectRatio;
                scaledHeight = image.Height;
                drawingPoint = new Point((int)((scaledWidth - image.Width) / 2), 0);
            }
            else if (boxAspectRatio < imageAspectRatio)
            {
                // image is wider than box
                scaledHeight = image.Width / boxAspectRatio;
                scaledWidth = image.Width;
                drawingPoint = new Point(0, (int)((scaledHeight - image.Height) / 2));
            }
            context.DrawImage(GraphicsOptions, image, drawingPoint);
            return GetProjectiveTransformationMatrix((float)scaledWidth, (float)scaledHeight);
        }

        private Matrix<float> ApplyStretchFit(IImageProcessingContext<Rgba32> context, Image<Rgba32> image)
        {
            context.DrawImage(GraphicsOptions, image, new Point(0, 0));
            return GetProjectiveTransformationMatrix(image.Width, image.Height);
        }

        private Matrix<float> ApplyTile(IImageProcessingContext<Rgba32> context, Image<Rgba32> image)
        {
            var numX = ((int) MaxWidth / image.Width) + 1;
            var numY = ((int) MaxHeight / image.Height) + 1;

            for (var i = 0; i < numX; i++)
            {
                for (var j = 0; j < numY; j++)
                {
                    context.DrawImage(GraphicsOptions, image, new Point(i * image.Width, j * image.Height));
                }
            }
            return GetProjectiveTransformationMatrix(MaxWidth, MaxHeight);
        }

        private Matrix<float> ApplyCenter(IImageProcessingContext<Rgba32> context, Image<Rgba32> image)
        {
            var sx = (MaxWidth - image.Width) / 2;
            var sy = (MaxHeight - image.Height) / 2;

            context.DrawImage(GraphicsOptions, image, new Point((int) sx, (int) sy));
            return GetProjectiveTransformationMatrix(MaxWidth, MaxHeight);
        }

        private async Task<Image<Rgba32>> GetImageFromUrl(string url)
        {
            var inImageCache = ImageCache.TryGet(url, out var image);
            if (inImageCache)
            {
                return image;
            }

            var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                throw new ArgumentException("Invalid URL provided.");
            }

            var client = new RestClient($"{uriResult.Scheme}://{uriResult.Authority}");
            var request = new RestRequest(uriResult.AbsolutePath);
            var response = await client.ExecuteGetTaskAsync(request);

            image = Image.Load(response.RawBytes);
            ImageCache.Add(url, image);
            return image;
        }
    }
}
