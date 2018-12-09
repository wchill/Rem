using System;
using System.Collections.Generic;
using System.Linq;
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
        private Point _topLeft;
        private Point _topRight;
        private Point _bottomLeft;
        private Point _bottomRight;
        private double _paddingPercentage;
        private readonly List<IInputRenderer> _renderers;
        private IReadOnlyList<Point> _mask;

        /*
        private bool _allowText;
        private Font _font;
        private IPen<Rgba32> _pen;
        private IBrush<Rgba32> _brush;
        private bool _centerWidth;
        private bool _centerHeight;
        private bool _preferNoScaling;
        
        private bool _allowImage;
        private ImageScalingMode PortraitScalingMode { get; set; } = ImageScalingMode.FitWithLetterbox;
        private ImageScalingMode LandscapeScalingMode { get; set; } = ImageScalingMode.FitWithLetterbox;
        */

        public InputFieldBuilder()
        {
            _paddingPercentage = 0;
            _renderers = new List<IInputRenderer>();

            /*
            _allowText = false;
            _font = MemeFonts.GetDefaultFont();
            _pen = null;
            _brush = Brushes.Solid(Rgba32.Black);
            _centerWidth = true;
            _centerHeight = true;
            _preferNoScaling = false;

            _allowImage = false;
            */
        }

        public InputFieldBuilder WithVertices(Point topLeft, Point topRight, Point bottomLeft, Point bottomRight)
        {
            _topLeft = topLeft;
            _topRight = topRight;
            _bottomLeft = bottomLeft;
            _bottomRight = bottomRight;
            return this;
        }

        public InputFieldBuilder WithPadding(double percentage)
        {
            _paddingPercentage = percentage;
            return this;
        }

        public InputFieldBuilder WithMask(IReadOnlyList<Point> shape)
        {
            if (shape.First() != shape.Last())
            {
                var newList = new List<Point>(shape) {shape.First()};
                shape = newList;
            }
            _mask = shape;
            return this;
        }

        public InputFieldBuilder WithMask(params Point[] shape)
        {
            return WithMask((IReadOnlyList<Point>) shape);
        }

        /*
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
        */

        public InputFieldBuilder WithRenderer(IInputRenderer renderer)
        {
            _renderers.Add(renderer);
            return this;
        }

        public InputField Build()
        {
            if (_topLeft == null || _topRight == null || _bottomLeft == null || _bottomRight == null)
            {
                throw new InvalidOperationException("Corners were not specified.");
            }

            if (!_renderers.Any())
            {
                throw new InvalidOperationException("At least one renderer must be specified.");
            }

            return new InputField(_topLeft, _topRight, _bottomLeft, _bottomRight, _paddingPercentage, _renderers, _mask);
        }
    }
}