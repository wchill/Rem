using System;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace MemeGenerator
{
    public class MemeTextRenderer
    {
        private readonly Font _font;
        private readonly IPen<Rgba32> _pen;
        private readonly IBrush<Rgba32> _brush;
        private readonly HorizontalAlignment _horizontalAlignment;
        private readonly VerticalAlignment _verticalAlignment;
        private readonly bool _preferNoScaling;

        public MemeTextRenderer(Font font, IPen<Rgba32> pen, IBrush<Rgba32> brush, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment,
            bool preferNoScaling)
        {
            _font = font;
            _pen = pen;
            _brush = brush;
            _horizontalAlignment = horizontalAlignment;
            _verticalAlignment = verticalAlignment;
            _preferNoScaling = preferNoScaling;
        }

        public Font GetAdjustedFont(int width, int height, string text)
        {
            if (_preferNoScaling && WillTextFit(width, height, text, _font))
            {
                return _font;
            }

            var bestFont = BinarySearchFontSize(width, height, text, _font);
            if (bestFont == null)
            {
                throw new ArgumentException("Unable to find a font size that fit the given constraints.");
            }

            return bestFont;
        }

        public void RenderTextToImage(IImageProcessingContext<Rgba32> context, Rectangle area, string text)
        {
            var width = area.Width;
            var height = area.Height;
            var x = area.X;
            var y = area.Y;

            var scaledFont = GetAdjustedFont(width, height, text);
            var textGraphicOptions = new TextGraphicsOptions(true)
            {
                HorizontalAlignment = _horizontalAlignment,
                VerticalAlignment = _verticalAlignment,
                WrapTextWidth = width
            };
            
            var renderY = _verticalAlignment == VerticalAlignment.Center ? height / 2 + y : y;

            context.DrawText(textGraphicOptions, text, scaledFont, _brush, _pen, new PointF(x, renderY));
        }

        private Font BinarySearchFontSize(int width, int height, string text, Font originalFont)
        {
            double low = 0.1;
            double high = height;

            Font bestFont = null;

            while (low <= high - 0.1)
            {
                var midSize = low + (high - low) / 2;
                var scaledFont = new Font(originalFont, (float) midSize);

                if (WillTextFit(width, height, text, scaledFont))
                {
                    if (bestFont == null || bestFont.Size < scaledFont.Size)
                    {
                        bestFont = scaledFont;
                    }
                    low = midSize + 0.1;
                }
                else
                {
                    high = midSize - 0.1;
                }
            }

            return bestFont;
        }

        private RectangleF GetTextBounds(int wrapWidth, string text, Font font)
        {
            return TextMeasurer.MeasureBounds(text, new RendererOptions(font)
            {
                HorizontalAlignment = _horizontalAlignment,
                VerticalAlignment = _verticalAlignment,
                WrappingWidth = wrapWidth
            });
        }

        private bool WillTextFit(int width, int height, string text, Font font)
        {
            var adjustedSizeNew = GetTextBounds(width, text, font);

            // Provide a little safety margin due to floating point error
            return height * 0.95 > adjustedSizeNew.Height && width * 0.95 > adjustedSizeNew.Width;
        }
    }
}