using Discord.Commands;
using MathNet.Numerics.LinearAlgebra;
using Rem.Bot;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rem.Commands.MemeGen
{
    static class ImageProcessingExtensions
    {
        private static Matrix<float> AdjugateMatrix(Matrix<float> matrix)
        {
            var adj = matrix.Clone();
            adj[0, 0] = matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1];
            adj[0, 1] = matrix[0, 2] * matrix[2, 1] - matrix[0, 1] * matrix[2, 2];
            adj[0, 2] = matrix[0, 1] * matrix[1, 2] - matrix[0, 2] * matrix[1, 1];
            adj[1, 0] = matrix[1, 2] * matrix[2, 0] - matrix[1, 0] * matrix[2, 2];
            adj[1, 1] = matrix[0, 0] * matrix[2, 2] - matrix[0, 2] * matrix[2, 0];
            adj[1, 2] = matrix[0, 2] * matrix[1, 0] - matrix[0, 0] * matrix[1, 2];
            adj[2, 0] = matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0];
            adj[2, 1] = matrix[0, 1] * matrix[2, 0] - matrix[0, 0] * matrix[2, 1];
            adj[2, 2] = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

            return adj;
        }

        private static Matrix<float> GetProjectiveTransformationMatrix(TextBoundingBox box)
        {
            var s = MapBasisToPoints(new PointF(0, 0), new PointF(box.MaxWidth, 0), new PointF(0, box.MaxHeight), new PointF(box.MaxWidth, box.MaxHeight));
            var d = MapBasisToPoints(
                box.TopLeft + new PointF(box.Padding, box.Padding),
                box.TopRight + new PointF(-box.Padding, box.Padding),
                box.BottomLeft + new PointF(box.Padding, -box.Padding),
                box.BottomRight + new PointF(-box.Padding, -box.Padding));
            var result = d.Multiply(AdjugateMatrix(s));
            var normalized = result.Divide(result[2, 2]);
            return normalized;
        }

        private static Matrix<float> MapBasisToPoints(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            var A = Matrix<float>.Build.DenseOfArray(new float[,]
            {
                {p1.X, p2.X, p3.X},
                {p1.Y, p2.Y, p3.Y},
                {1, 1, 1}
            });
            var b = Vector<float>.Build.Dense(new float[] { p4.X, p4.Y, 1 });
            var aj = AdjugateMatrix(A);
            var v = aj.Multiply(b);
            var m = Matrix<float>.Build.DenseOfArray(new float[,]
            {
                {v[0], 0, 0 },
                {0, v[1], 0 },
                {0, 0, v[2] }
            });
            return A.Multiply(m);
        }
 
        public static IImageProcessingContext<Rgba32> ApplyTextWithBoundingBoxes(this IImageProcessingContext<Rgba32> context, Font[] scaledFonts, TextBoundingBox[] boundingBoxes, string[] text)
        {
            for (var i = 0; i < boundingBoxes.Length; i++)
            {
                context = ApplyTextWithBoundingBox(context, scaledFonts[i], boundingBoxes[i], text[i]);
            }
            return context;
        }

        public static IImageProcessingContext<Rgba32> ApplyTextWithBoundingBox(this IImageProcessingContext<Rgba32> context, Font scaledFont, TextBoundingBox boundingBox, string text)
        {
            return context.Apply(img =>
            {
                var py = boundingBox.CenterHeight ? boundingBox.MaxHeight / 2 : 0;

                var hAlign = boundingBox.CenterWidth ? HorizontalAlignment.Center : HorizontalAlignment.Left;
                var vAlign = boundingBox.CenterHeight ? VerticalAlignment.Center : VerticalAlignment.Top;
                var textGraphicOptions = new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = hAlign,
                    VerticalAlignment = vAlign,
                    WrapTextWidth = boundingBox.MaxWidth
                };

                var textCanvas = new Image<Rgba32>(img.Width, img.Height);
                textCanvas.Mutate(i =>
                {
                    i.DrawText(textGraphicOptions, text, scaledFont, boundingBox.Brush, boundingBox.Pen, new PointF(0, py));
                    var transformMatrix = GetProjectiveTransformationMatrix(boundingBox);
                    var matrix4x4 = new System.Numerics.Matrix4x4(
                        transformMatrix[0, 0], transformMatrix[1, 0], 0, transformMatrix[2, 0],
                        transformMatrix[0, 1], transformMatrix[1, 1], 0, transformMatrix[2, 1],
                        0, 0, 1, 0,
                        transformMatrix[0, 2], transformMatrix[1, 2], 0, transformMatrix[2, 2]
                    );
                    i.Transform(matrix4x4, KnownResamplers.Lanczos3);
                    foreach (var mask in boundingBox.Masks)
                    {
                        i.Opacity(0, mask);
                    }
                });

                img.Mutate(i => i.DrawImage(textCanvas, 1.0f, new Point(0, 0)));
            });
        }
    }

    public class TextMemeTemplate
    {
        private static readonly string ImageFolder = @"Images\MemeTemplates\";
        private readonly string _imagePath;
        private readonly TextBoundingBox[] _boundingBoxes;

        public TextMemeTemplate(string filename, params TextBoundingBox[] boundingBoxes)
        {
            _imagePath = ImageFolder + filename;
            _boundingBoxes = boundingBoxes;
        }

        public async Task<string> GenerateImage(ICommandContext context, params string[] text)
        {
            if (text.Length != _boundingBoxes.Length)
            {
                throw new ArgumentException($"Expected {_boundingBoxes.Length} fields but got {text.Length}.");
            }

            if (text.Length == 1 && text[0].Length > 2 && text[0].First() == '\"' && text[0].Last() == '\"')
            {
                text[0] = text[0].Substring(1, text[0].Length - 2);
            }

            using (var img = Image.Load(_imagePath))
            {
                for (var i = 0; i < text.Length; i++)
                {
                    text[i] = await context.Guild.ResolveIds(text[i]);
                }

                var scaledFonts = await Task.Run(() => CalculateScaleSize(img, _boundingBoxes, text));
                return await Task.Run(() =>
                {
                    using (var img2 = img.Clone(i => i.ApplyTextWithBoundingBoxes(scaledFonts, _boundingBoxes, text)))
                    {
                        var filename = Guid.NewGuid().ToString() + ".png";
                        img2.Save(filename);
                        return filename;
                    }
                });
            }
        }

        private static Font[] CalculateScaleSize(Image<Rgba32> img, TextBoundingBox[] boundingBoxes, string[] texts)
        {
            var fonts = new Font[boundingBoxes.Length];
            for (var i = 0; i < fonts.Length; i++)
            {
                var boundingBox = boundingBoxes[i];
                var text = texts[i];

                var targetWidth = boundingBox.MaxWidth;
                var targetHeight = boundingBox.MaxHeight;

                var targetMinHeight = targetHeight * 0.75f;
                var scaledFont = boundingBox.Font;

                var size = TextMeasurer.Measure(text, new RendererOptions(scaledFont)
                {
                    WrappingWidth = targetWidth
                });

                if (boundingBox.ForceNoScaling || boundingBox.PreferNoScaling)
                {
                    if (boundingBox.ForceNoScaling && (size.Width > targetWidth || size.Height > targetHeight))
                    {
                        throw new ArgumentException("Unable to fit input into meme because it was too long.");
                    }
                    else if (!(size.Width > targetWidth || size.Height > targetHeight))
                    {
                        fonts[i] = scaledFont;
                        continue;
                    }
                }
                var longestWord = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => TextMeasurer.Measure(w, new RendererOptions(scaledFont)
                {
                    WrappingWidth = targetWidth
                })).OrderByDescending(s => s.Width).First();

                var scaleFactor = scaledFont.Size;
                var minScaleFactor = 0.1f;

                if (size.Height < targetHeight && size.Width < targetWidth)
                {
                    while (size.Height < targetHeight && size.Width < targetWidth)
                    {
                        while (scaleFactor > minScaleFactor)
                        {
                            scaledFont = new Font(scaledFont, scaledFont.Size + scaleFactor);
                            size = TextMeasurer.Measure(text, new RendererOptions(scaledFont)
                            {
                                WrappingWidth = targetWidth
                            });

                            if (size.Height > targetHeight || size.Width > targetWidth)
                            {
                                scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
                                scaleFactor = Math.Max(minScaleFactor, scaleFactor / 2);
                            }
                        }
                    }
                }
                else
                {
                    while (size.Height > targetHeight || size.Width > targetWidth)
                    {
                        while (scaleFactor > minScaleFactor)
                        {
                            scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
                            size = TextMeasurer.Measure(text, new RendererOptions(scaledFont)
                            {
                                WrappingWidth = targetWidth
                            });

                            if (size.Height < targetHeight && size.Width < targetWidth)
                            {
                                scaledFont = new Font(scaledFont, scaledFont.Size + scaleFactor);
                                scaleFactor = Math.Max(minScaleFactor, scaleFactor / 2);
                            }
                        }
                    }
                    scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
                }

                size = TextMeasurer.Measure(text, new RendererOptions(scaledFont)
                {
                    WrappingWidth = targetWidth
                });

                if (size.Width > targetWidth || size.Height > targetHeight)
                {
                    throw new ArgumentException("Unable to fit input into meme because it was too long.");
                }
                fonts[i] = scaledFont;
            }

            return fonts;
        }
    }
}
