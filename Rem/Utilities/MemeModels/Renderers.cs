using MemeGenerator;
using MemeGenerator.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System;

namespace Rem.Utilities.MemeModels
{
    class TextRenderer
    {
        public AvailableFonts Font { get; set; }
        public int FontSize { get; set; } = 20;
        public string Pen { get; set; }
        public int PenWidth { get; set; }
        public string Brush { get; set; }
        public string[] Alignment { get; set; }
        public bool PreferNoScaling { get; set; }

        public TextInputRenderer GetRenderer()
            => new TextInputRenderer(
                MemeFonts.GetFont(Font, FontSize),
                Pen.Hex().GetPen(PenWidth),
                Brush.Hex().GetBrush(),
                Enum.Parse<HorizontalAlignment>(Alignment[0]),
                Enum.Parse<VerticalAlignment>(Alignment[1]),
                PreferNoScaling);
    }

    class ImageRenderer
    {
        public ImageScalingMode ScalingMode { get; set; }
        public GraphicsOptions GraphicsOptions { get; set; }

        public ImageInputRenderer GetRenderer()
            => new ImageInputRenderer(ScalingMode, GraphicsOptions);
    }
}
