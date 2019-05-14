using System;
using System.Linq;
using System.Runtime.Serialization;
using MemeGenerator.JsonConverters;
using Newtonsoft.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace MemeGenerator
{
    public class TextInputRenderer : IInputRenderer
    {
        [JsonConverter(typeof(FontJsonConverter))]
        public Font Font { get; }
        [JsonConverter(typeof(PenJsonConverter))]
        public IPen<Rgba32> Pen { get; }
        public IBrush<Rgba32> Brush { get; }
        public HorizontalAlignment HorizontalAlignment { get; }
        public VerticalAlignment VerticalAlignment { get; }
        public bool PreferNoScaling { get; }

        public TextInputRenderer(Font font, IPen<Rgba32> pen, IBrush<Rgba32> brush, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment,
            bool preferNoScaling)
        {
            Font = font;
            Pen = pen;
            Brush = brush;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            PreferNoScaling = preferNoScaling;
        }

        public Font GetAdjustedFont(int width, int height, string text)
        {
            if (PreferNoScaling && WillTextFit(width, height, text, Font))
            {
                return Font;
            }

            var bestFont = BinarySearchFontSize(width, height, text, Font);
            if (bestFont == null)
            {
                throw new ArgumentException("Unable to find a font size that fit the given constraints.");
            }

            return bestFont;
        }

        public bool Render(IImageProcessingContext<Rgba32> context, Rectangle area, object input)
        {
            if (!(input is string text))
            {
                return false;
            }

            var width = area.Width;
            var height = area.Height;
            var x = area.X;
            var y = area.Y;

            var scaledFont = GetAdjustedFont(width, height, text);
            var textGraphicOptions = new TextGraphicsOptions(true)
            {
                HorizontalAlignment = HorizontalAlignment,
                VerticalAlignment = VerticalAlignment,
                WrapTextWidth = width
            };
            
            var renderY = VerticalAlignment == VerticalAlignment.Center ? height / 2 + y : y;

            context.DrawText(textGraphicOptions, text, scaledFont, Brush, Pen, new PointF(x, renderY));

            return true;
        }

        private Font BinarySearchFontSize(int width, int height, string text, Font originalFont)
        {
            double low = 0.1;
            double high = height / 2;

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
                HorizontalAlignment = HorizontalAlignment,
                VerticalAlignment = VerticalAlignment,
                WrappingWidth = wrapWidth
            });
        }

        private bool WillTextFit(int width, int height, string text, Font font)
        {
            var adjustedSizeNew = GetTextBounds((int) (width * 0.95), text, font);

            // Provide a little safety margin due to floating point error
            return height * 0.95 > adjustedSizeNew.Height && width * 0.95 > adjustedSizeNew.Width;
        }
    }
}