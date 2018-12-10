using MathNet.Numerics.LinearAlgebra;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MemeGenerator
{
    public class InputField
    {
        public string Name { get; }
        public string Description { get; }
        public Point TopLeft { get; }
        public Point TopRight { get; }
        public Point BottomLeft { get; }
        public Point BottomRight { get; }
        public double PaddingPercent { get; }
        public IReadOnlyList<Point> Mask { get; }
        private readonly IReadOnlyList<IInputRenderer> _renderers;

        public int WidthTop => ApplyPadding2X(TopRight.X - TopLeft.X);
        public int WidthBottom => ApplyPadding2X(BottomRight.X - BottomLeft.X);
        public int HeightLeft => ApplyPadding2X(BottomLeft.Y - TopLeft.Y);
        public int HeightRight => ApplyPadding2X(BottomRight.Y - TopRight.Y);
        public int MaxWidth => Math.Max(WidthTop, WidthBottom);
        public int MaxHeight => Math.Max(HeightLeft, HeightRight);
        public Rectangle DrawingArea => new Rectangle(0, 0, MaxWidth, MaxHeight);

        public InputField(string name, string description, Point topLeft, Point topRight, Point bottomLeft, Point bottomRight, double paddingPercent, IReadOnlyList<IInputRenderer> renderers, IReadOnlyList<Point> mask = null)
        {
            Name = name;
            Description = description;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            PaddingPercent = paddingPercent;
            Mask = mask ?? new[] { topLeft, topRight, bottomLeft, bottomRight };
            _renderers = renderers;

            if (TopLeft.X < 0 || TopLeft.Y < 0 || topRight.X < 0 || topRight.Y < 0 || bottomLeft.X < 0 || bottomLeft.Y < 0 || bottomRight.X < 0 || bottomRight.Y < 0)
            {
                throw new ArgumentException("Coordinates cannot be less than 0.");
            }

            if (paddingPercent < 0 || paddingPercent > 1)
            {
                throw new ArgumentException("Padding percentage must be between 0 and 1 inclusive.");
            }

            if (!_renderers.Any())
            {
                throw new ArgumentException("At least one renderer must be specified.");
            }
        }

        public bool Apply(IImageProcessingContext<Rgba32> context, object input)
        {
            var success = false;
            foreach (var renderer in _renderers)
            {
                // Attempt to render this input with each renderer until one succeeds
                success = renderer.Render(context, DrawingArea, input);
                if (success) break;
            }

            return success;
        }

        private int ApplyPadding2X(int val)
        {
            return (int)((1 - 2 * PaddingPercent) * val);
        }

        private int ApplyPadding(int val)
        {
            return (int)((1 - PaddingPercent) * val);
        }
    }
}
