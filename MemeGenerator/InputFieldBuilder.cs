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
        private string _name;
        private string _description;
        private Point _topLeft;
        private Point _topRight;
        private Point _bottomLeft;
        private Point _bottomRight;
        private double _paddingPercentage;
        private readonly List<IInputRenderer> _renderers;
        private IReadOnlyList<Point> _mask;
        public InputFieldBuilder()
        {
            _paddingPercentage = 0;
            _renderers = new List<IInputRenderer>();
        }

        public InputFieldBuilder WithName(string name, string description = null)
        {
            _name = name;
            _description = description ?? "";
            return this;
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

            if (_name == null)
            {
                throw new InvalidOperationException("Name was not specified.");
            }

            return new InputField(_name, _description, _topLeft, _topRight, _bottomLeft, _bottomRight, _paddingPercentage, _renderers, _mask);
        }
    }
}