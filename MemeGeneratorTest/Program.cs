using MemeGenerator;
using MemeGenerator.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;

namespace MemeGeneratorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var imageRenderer = new ImageInputRenderer(ImageScalingMode.FitWithLetterbox, new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 8
            });

            var font = MemeFonts.GetDefaultFont();
            var textRenderer = new TextInputRenderer(font, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Left, VerticalAlignment.Top, false);
            
            var inputField = new InputFieldBuilder()
                .WithName("Paper", "Whatever Komi's looking at on the paper")
                .WithVertices(new Point(245, 479), new Point(707, 563), new Point(48, 980), new Point(557, 1067))
                .WithRenderer(imageRenderer)
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .WithMask(new Point(245, 479), new Point(665, 556), new Point(665, 699), new Point(555, 1066), new Point(184, 1068), new Point(184, 633))
                .Build();

            var memeTemplate = new MemeTemplateBuilder("KomiPaper.png")
                .WithInputField(inputField)
                .Build();
            using (memeTemplate)
            {
                var img = memeTemplate.CreateMeme(
                    new object[] {"test with multiple words in a sentence blah blah blah test with multiple words in a sentence blah blah blah test with multiple words in a sentence blah blah blah "});
                using (img)
                {
                    img.Save("output.png");
                }
            }
            
        }

        public static void TestRendering()
        {
            var testTextData = new[]
{
                Tuple.Create(100, 100, "test"),
                Tuple.Create(1000, 1000, "test"),
                Tuple.Create(100, 100, "testwithamuchlongerstringblahblahblahtestwithamuchlongerstringblahblahblah"),
                Tuple.Create(1000, 1000, "testwithamuchlongerstringblahblahblahtestwithamuchlongerstringblahblahblah"),
                Tuple.Create(100, 100, "test with multiple words in a sentence blah blah blah test with multiple words in a sentence blah blah blah test with multiple words in a sentence blah blah blah "),
                Tuple.Create(1000, 1000, "test with multiple words in a sentence blah blah blah test with multiple words in a sentence blah blah blah test with multiple words in a sentence blah blah blah ")
            };

            for (var i = 0; i < testTextData.Length; i++)
            {
                var data = testTextData[i];
                using (var img = TextRenderingWithinBoundaries(data.Item1, data.Item2, data.Item3))
                {
                    img.Save($"text-{i}.png");
                }
            }

            var testImageData = new[]
            {
                Tuple.Create(100, 100),
                Tuple.Create(1000, 1000),
                Tuple.Create(2000, 1000),
                Tuple.Create(1000, 2000),
                Tuple.Create(1000, 100),
                Tuple.Create(100, 1000),
            };

            for (var i = 0; i < testImageData.Length; i++)
            {
                var modes = new[]
                {
                    ImageScalingMode.None,
                    ImageScalingMode.Center,
                    ImageScalingMode.FillFit,
                    ImageScalingMode.FitWithLetterbox,
                    ImageScalingMode.StretchFit
                };

                var data = testImageData[i];
                for (var j = 0; j < modes.Length; j++)
                {
                    using (var img = ImageRenderingWithinBoundaries(data.Item1, data.Item2, "KomiPaper.png", modes[j]))
                    {
                        img.Save($"img-{i}-{modes[j].ToString()}.png");
                    }
                }
            }
        }

        public static Image<Rgba32> ImageRenderingWithinBoundaries(int width, int height, string file, ImageScalingMode mode)
        {
            var margin = 20;

            var canvas = new Image<Rgba32>(width + 2 * margin, height + 2 * margin);
            var renderer = new ImageInputRenderer(mode, new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 8
            });
            var renderArea = new Rectangle(margin, margin, width, height);
            canvas.Mutate(ctx =>
            {
                ctx.Fill(Rgba32.AliceBlue);
                ctx.Fill(Rgba32.White, renderArea);
                using (var img = Image.Load(file))
                {
                    renderer.Render(ctx, renderArea, img);
                }
            });

            return canvas;
        }

        public static Image<Rgba32> TextRenderingWithinBoundaries(int width, int height, string text)
        {
            var margin = 20;

            var canvas = new Image<Rgba32>(width + 2 * margin, height + 2 * margin);
            var font = MemeFonts.GetDefaultFont();
            var renderer = new TextInputRenderer(font, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Center, VerticalAlignment.Center, false);
            var renderArea = new Rectangle(margin, margin, width, height);
            canvas.Mutate(ctx =>
            {
                ctx.Fill(Rgba32.AliceBlue);
                ctx.Fill(Rgba32.White, renderArea);
                renderer.Render(ctx, renderArea, text);
            });

            return canvas;
        }
    }
}
