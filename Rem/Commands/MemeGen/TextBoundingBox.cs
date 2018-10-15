using MathNet.Numerics.LinearAlgebra;
using SixLabors.Fonts;
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
    public class TextBoundingBox : BaseBoundingBox
    {
        public IPen<Rgba32> Pen { get; set; } = null;
        public IBrush<Rgba32> Brush { get; set; } = Brushes.Solid(Rgba32.Black);
        public Font Font { get; set; } = SystemFonts.CreateFont("Arial", 20);
        public bool CenterWidth { get; set; } = true;
        public bool CenterHeight { get; set; } = true;
        public bool ForceNoScaling { get; set; } = false;
        public bool PreferNoScaling { get; set; } = false;

        public TextBoundingBox()
        {

        }

        public TextBoundingBox(float x, float y, float w, float h)
        {
            TopLeft = new PointF(x, y);
            TopRight = new PointF(x + w, y);
            BottomLeft = new PointF(x, y + h);
            BottomRight = new PointF(x + w, y + h);
        }

        public override Task<bool> CanHandleAsync(string input)
        {
            return Task.FromResult(true);
        }

        public override void SetInput(string input)
        {
            base.SetInput(input);
        }

        internal override Task<Matrix<float>> ApplyAsyncInternal(IImageProcessingContext<Rgba32> context)
        {
            if (_lastInput == null)
            {
                throw new InvalidOperationException("Input cannot be null.");
            }

            var scaledFont = CalculateScaleSize(_lastInput);

            var py = CenterHeight ? MaxHeight / 2 : 0;

            var hAlign = CenterWidth ? HorizontalAlignment.Center : HorizontalAlignment.Left;
            var vAlign = CenterHeight ? VerticalAlignment.Center : VerticalAlignment.Top;
            var textGraphicOptions = new TextGraphicsOptions(true)
            {
                HorizontalAlignment = hAlign,
                VerticalAlignment = vAlign,
                WrapTextWidth = MaxWidth
            };

            context.DrawText(textGraphicOptions, _lastInput, scaledFont, Brush, Pen, new PointF(0, py));

            return Task.FromResult(GetProjectiveTransformationMatrix());
        }

        private Font CalculateScaleSize(string text)
        {
            var targetWidth = MaxWidth;
            var targetHeight = MaxHeight;

            var targetMinHeight = targetHeight * 0.75f;
            var scaledFont = Font;

            var size = TextMeasurer.Measure(text, new RendererOptions(scaledFont)
            {
                WrappingWidth = targetWidth
            });

            if (ForceNoScaling || PreferNoScaling)
            {
                if (ForceNoScaling && (size.Width > targetWidth || size.Height > targetHeight))
                {
                    throw new ArgumentException("Unable to fit input into meme because it was too long.");
                }
                else if (!(size.Width > targetWidth || size.Height > targetHeight))
                {
                    return scaledFont;
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
            return scaledFont;
        }
    }
}
