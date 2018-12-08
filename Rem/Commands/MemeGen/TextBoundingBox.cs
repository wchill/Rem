﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Rem.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Rem.Commands.MemeGen
{
    public class TextBoundingBox : BaseBoundingBox
    {
        public static readonly Font DefaultFont = InitializeDefaultFont();

        public IPen<Rgba32> Pen { get; set; }
        public IBrush<Rgba32> Brush { get; set; } = Brushes.Solid(Rgba32.Black);
        public Font Font { get; set; } = DefaultFont;
        public bool CenterWidth { get; set; } = true;
        public bool CenterHeight { get; set; } = true;
        public bool PreferNoScaling { get; set; }

        static Font InitializeDefaultFont()
        {
            var collection = new FontCollection();
            collection.Install(FontNames.OpenSans, out var fontDescription);
            return collection.CreateFont(fontDescription.FontFamily, 20);
        }

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

            //Not used
            var targetMinHeight = targetHeight * 0.75f;

            var scaledFont = Font;

            var size = TextMeasurer.Measure(text, new RendererOptions(scaledFont)
            {
                WrappingWidth = targetWidth
            });

            if (PreferNoScaling)
            {
                if (!(size.Width > targetWidth || size.Height > targetHeight))
                {
                    return scaledFont;
                }
            }

            //Not used
            var longestWord = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => TextMeasurer.Measure(w, new RendererOptions(scaledFont)
                {
                    WrappingWidth = targetWidth
                })).OrderByDescending(s => s.Width).First();

            var scaleFactor = scaledFont.Size / 2;
            const float minScaleFactor = 0.1f;

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

                        if (!(size.Height > targetHeight) && !(size.Width > targetWidth)) continue;
                        scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
                        scaleFactor = Math.Max(minScaleFactor, scaleFactor / 2);
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

                        if (!(size.Height < targetHeight) || !(size.Width < targetWidth)) continue;
                        scaledFont = new Font(scaledFont, scaledFont.Size + scaleFactor);
                        scaleFactor = Math.Max(minScaleFactor, scaleFactor / 2);
                    }
                }

                scaledFont = new Font(scaledFont, scaledFont.Size - (scaleFactor * 2));
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
