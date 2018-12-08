using System;
using System.Collections.Generic;
using MemeGenerator.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace MemeGenerator
{
    public class InputFieldBuilder
    {
        private PointF[] _corners;
        private readonly List<PointF[]> _drawableArea;
        private double _paddingPercentage;

        private bool _allowText;
        private Font _font;
        private IPen<Rgba32> _pen;
        private IBrush<Rgba32> _brush;
        private bool _centerWidth;
        private bool _centerHeight;
        private bool _preferNoScaling;

        /*
        private bool _allowImage;
        private ImageScalingMode PortraitScalingMode { get; set; } = ImageScalingMode.FitWithLetterbox;
        private ImageScalingMode LandscapeScalingMode { get; set; } = ImageScalingMode.FitWithLetterbox;
        */

        public InputFieldBuilder()
        {
            _drawableArea = new List<PointF[]>();
            _paddingPercentage = 0;
            _allowText = false;
            _font = MemeFonts.GetDefaultFont();
            _pen = null;
            _brush = Brushes.Solid(Rgba32.Black);
            _centerWidth = true;
            _centerHeight = true;
            _preferNoScaling = false;

            //_allowImage = false;
        }

        public InputFieldBuilder WithVertices(Point topLeft, Point topRight, Point bottomLeft, Point bottomRight)
        {
            return WithVertices(
                new PointF(topLeft.X, topLeft.Y),
                new PointF(topRight.X, topRight.Y),
                new PointF(bottomLeft.X, bottomLeft.Y),
                new PointF(bottomRight.X, bottomRight.Y));
        }

        public InputFieldBuilder WithVertices(PointF topLeft, PointF topRight, PointF bottomLeft, PointF bottomRight)
        {
            _corners = new[] { topLeft, topRight, bottomLeft, bottomRight };
            return this;
        }

        public InputFieldBuilder WithPadding(double percentage)
        {
            _paddingPercentage = percentage;
            return this;
        }

        public InputFieldBuilder WithMask(PointF[] shape)
        {
            _drawableArea.Add(shape);
            return this;
        }

        public InputFieldBuilder EnableText()
        {
            _allowText = true;
            return this;
        }

        public InputFieldBuilder WithFont(AvailableFonts fontName, float size = 20)
        {
            _font = MemeFonts.GetFont(fontName, size);
            return this;
        }

        public InputFieldBuilder WithPen(IPen<Rgba32> pen)
        {
            _pen = pen;
            return this;
        }

        public InputFieldBuilder WithBrush(IBrush<Rgba32> brush)
        {
            _brush = brush;
            return this;
        }

        public InputFieldBuilder WithTextLayoutSettings(bool centerWidth = true, bool centerHeight = true, bool preferNoScaling = false)
        {
            _centerWidth = centerWidth;
            _centerHeight = centerHeight;
            _preferNoScaling = preferNoScaling;
            return this;
        }

        public InputField Build()
        {
            if (_corners == null)
            {
                throw new InvalidOperationException("Corners were not specified.");
            }

            if (!_allowText)
            {
                throw new InvalidOperationException("At least one input mode must be specified for the bounding box.");
            }

            return new InputField(_corners[0], _corners[1], _corners[2], _corners[3], _paddingPercentage);
        }
    }
}