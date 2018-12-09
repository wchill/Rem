using System.Collections.Generic;
using MemeGenerator;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Xunit;

namespace Tests
{
    public class ImageRenderingTests
    {
        public static IEnumerable<object[]> BoundaryTestData =>
            new List<object[]>
            {
                // Square box, happy path (same dimensions)
                new object[] { 10, 10, 10, 10 },

                // Square box, image smaller than box
                new object[] { 5, 5, 10, 10 },
                new object[] { 10, 5, 10, 10 },
                new object[] { 5, 10, 10, 10 },

                // Square box, image larger than box
                new object[] { 20, 5, 10, 10 },
                new object[] { 5, 20, 10, 10 },
                new object[] { 20, 20, 10, 10 },

                // Wide box, happy path (same dimensions)
                new object[] { 15, 10, 15, 10 },

                // Wide box, image smaller than box
                new object[] { 5, 5, 15, 10 },
                new object[] { 15, 5, 15, 10 },
                new object[] { 5, 10, 15, 10 },

                // Wide box, image larger than box
                new object[] { 20, 5, 15, 10 },
                new object[] { 5, 20, 15, 10 },
                new object[] { 20, 20, 15, 10 },

                // Tall box, happy path (same dimensions)
                new object[] { 10, 15, 10, 15 },

                // Tall box, image smaller than box
                new object[] { 5, 5, 10, 15 },
                new object[] { 10, 5, 10, 15 },
                new object[] { 5, 15, 10, 15 },

                // Tall box, image larger than box
                new object[] { 20, 5, 10, 15 },
                new object[] { 5, 20, 10, 15 },
                new object[] { 20, 20, 10, 15 },
            };

        [Theory]
        [MemberData(nameof(BoundaryTestData))]
        public void ImageRenderingWithinBoundariesNoScaling(int imgWidth, int imgHeight, int boxWidth, int boxHeight)
        {
            var renderer = new MemeImageRenderer(ImageScalingMode.None, new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 8
            });
            CheckImageRendering(renderer, imgWidth, imgHeight, boxWidth, boxHeight);
        }

        [Theory]
        [MemberData(nameof(BoundaryTestData))]
        public void ImageRenderingWithinBoundariesCentering(int imgWidth, int imgHeight, int boxWidth, int boxHeight)
        {
            var renderer = new MemeImageRenderer(ImageScalingMode.Center, new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 8
            });
            CheckImageRendering(renderer, imgWidth, imgHeight, boxWidth, boxHeight);
        }

        [Theory]
        [MemberData(nameof(BoundaryTestData))]
        public void ImageRenderingWithinBoundariesStretchFit(int imgWidth, int imgHeight, int boxWidth, int boxHeight)
        {
            var renderer = new MemeImageRenderer(ImageScalingMode.StretchFit, new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 8
            });
            CheckImageRendering(renderer, imgWidth, imgHeight, boxWidth, boxHeight);
        }

        [Theory(Skip="Not implemented yet")]
        [MemberData(nameof(BoundaryTestData))]
        public void ImageRenderingWithinBoundariesTiling(int imgWidth, int imgHeight, int boxWidth, int boxHeight)
        {
            var renderer = new MemeImageRenderer(ImageScalingMode.Tile, new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 8
            });
            CheckImageRendering(renderer, imgWidth, imgHeight, boxWidth, boxHeight);
        }

        [Theory]
        [MemberData(nameof(BoundaryTestData))]
        public void ImageRenderingWithinBoundariesFillFit(int imgWidth, int imgHeight, int boxWidth, int boxHeight)
        {
            var renderer = new MemeImageRenderer(ImageScalingMode.FillFit, new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 8
            });
            CheckImageRendering(renderer, imgWidth, imgHeight, boxWidth, boxHeight);
        }

        [Theory]
        [MemberData(nameof(BoundaryTestData))]
        public void ImageRenderingWithinBoundariesFitWithLetterbox(int imgWidth, int imgHeight, int boxWidth, int boxHeight)
        {
            var renderer = new MemeImageRenderer(ImageScalingMode.FitWithLetterbox, new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 8
            });
            CheckImageRendering(renderer, imgWidth, imgHeight, boxWidth, boxHeight);
        }

        private static void CheckImageRendering(MemeImageRenderer renderer, int imgWidth, int imgHeight, int boxWidth, int boxHeight)
        {
            const int margin = 20;

            using (var canvas = new Image<Rgba32>(boxWidth + 2 * margin, boxHeight + 2 * margin))
            {
                var renderArea = new Rectangle(margin, margin, boxWidth, boxHeight);
                canvas.Mutate(ctx =>
                {
                    ctx.Fill(Rgba32.AliceBlue);
                    using (var img = GenerateImage(imgWidth, imgHeight))
                    {
                        renderer.DrawImageToImage(ctx, renderArea, img);
                    }
                });
                for (var y = 0; y < canvas.Height; y++)
                {
                    for (var x = 0; x < margin; x++)
                    {
                        Assert.True(canvas[x, y] == Rgba32.AliceBlue, $"Image was rendered outside the drawing canvas at ({x}, {y})");
                    }
                    if (y < margin || y >= canvas.Height - margin)
                    {
                        for (var x = margin; x < canvas.Width - margin; x++)
                        {
                            Assert.True(canvas[x, y] == Rgba32.AliceBlue, $"Image was rendered outside the drawing canvas at ({x}, {y})");
                        }
                    }
                    for (var x = canvas.Width - margin; x < canvas.Width; x++)
                    {
                        Assert.True(canvas[x, y] == Rgba32.AliceBlue, $"Image was rendered outside the drawing canvas at ({x}, {y})");
                    }
                }
            }
        }

        private static Image<Rgba32> GenerateImage(int width, int height)
        {
            var img = new Image<Rgba32>(width, height);
            img.Mutate(ctx => ctx.Fill(Rgba32.PaleVioletRed));
            return img;
        }
    }
}
