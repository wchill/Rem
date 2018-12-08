using MemeGenerator;
using System;
using MemeGenerator.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Xunit;

namespace Tests
{
    public class TextRenderingTests
    {
        [Theory]
        [InlineData(100, 100, "test")]
        [InlineData(100, 100, "testwithalongerstring")]
        [InlineData(1000, 1000, "testwithalongerstring")]
        [InlineData(1000, 1000, "test with multiple words that require word wrap")]
        public void TextRenderingWithinBoundaries(int width, int height, string text)
        {
            var margin = 20;

            using (var canvas = new Image<Rgba32>(width + 2 * margin, height + 2 * margin))
            {
                var font = MemeFonts.GetDefaultFont();
                var renderer = new MemeTextRenderer(font, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Center, VerticalAlignment.Center, false);
                var renderArea = new Rectangle(margin, margin, width, height);
                canvas.Mutate(ctx =>
                {
                    ctx.Fill(Rgba32.AliceBlue);
                    renderer.RenderTextToImage(ctx, renderArea, text);
                });
                for (var y = 0; y < canvas.Height; y++)
                {
                    for (var x = 0; x < margin; x++)
                    {
                        Assert.True(canvas[x, y] == Rgba32.AliceBlue, $"Text was rendered outside the drawing canvas at ({x}, {y})");
                    }
                    if (y < margin || y >= canvas.Height - margin)
                    {
                        for (var x = margin; x < canvas.Width - margin; x++)
                        {
                            Assert.True(canvas[x, y] == Rgba32.AliceBlue, $"Text was rendered outside the drawing canvas at ({x}, {y})");
                        }
                    }
                    for (var x = canvas.Width - margin; x < canvas.Width; x++)
                    {
                        Assert.True(canvas[x, y] == Rgba32.AliceBlue, $"Text was rendered outside the drawing canvas at ({x}, {y})");
                    }
                }
            }
        }
    }
}
