using System;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MemeGenerator
{
    public class MemeTextRenderer
    {
        private readonly Font _font;
        private readonly IPen<Rgba32> _pen;
        private readonly IBrush<Rgba32> _brush;
        private readonly bool _centerWidth;
        private readonly bool _centerHeight;
        private readonly bool _preferNoScaling;

        public MemeTextRenderer(Font font, IPen<Rgba32> pen, IBrush<Rgba32> brush, bool centerWidth, bool centerHeight,
            bool preferNoScaling)
        {
            _font = font;
            _pen = pen;
            _brush = brush;
            _centerWidth = centerWidth;
            _centerHeight = centerHeight;
            _preferNoScaling = preferNoScaling;
        }

        public Font GetAdjustedFont(float width, float height, string text)
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

        private static Font BinarySearchFontSize(float width, float height, string text, Font originalFont)
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

        private static bool WillTextFit(float width, float height, string text, Font font)
        {
            var adjustedSizeNew = TextMeasurer.Measure(text, new RendererOptions(font)
            {
                WrappingWidth = width
            });

            return height > adjustedSizeNew.Height && width > adjustedSizeNew.Width;
        }
    }
}