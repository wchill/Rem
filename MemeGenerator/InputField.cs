using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace MemeGenerator
{
    public enum ImageScalingMode
    {
        None,
        FillFit,
        FitWithLetterbox,
        StretchFit,
        Tile,
        Center
    }

    public class InputField
    {
        public PointF TopLeft { get; }
        public PointF TopRight { get; }
        public PointF BottomLeft { get; }
        public PointF BottomRight { get; }
        public double PaddingPercent { get; }

        public float WidthTop => ApplyPadding2X(TopRight.X - TopLeft.X);

        public float WidthBottom => ApplyPadding2X(BottomRight.X - BottomLeft.X);

        public float HeightLeft => ApplyPadding2X(BottomLeft.Y - TopLeft.Y);

        public float HeightRight => ApplyPadding2X(BottomRight.Y - TopRight.Y);

        public float MaxWidth => Math.Max(WidthTop, WidthBottom);

        public float MaxHeight => Math.Max(HeightLeft, HeightRight);

        public PointF BoundingBoxTopLeft => new PointF(ApplyPadding(Math.Min(TopLeft.X, BottomLeft.X)), ApplyPadding(Math.Min(TopLeft.Y, TopRight.Y)));

        public PointF BoundingBoxBottomRight => new PointF(ApplyPadding(Math.Max(TopRight.X, BottomRight.X)), ApplyPadding(Math.Max(BottomLeft.Y, BottomRight.Y)));

        public InputField(PointF topLeft, PointF topRight, PointF bottomLeft, PointF bottomRight, double paddingPercent)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            PaddingPercent = paddingPercent;
        }

        public void Apply(IImageProcessingContext<Rgba32> context)
        {
            Matrix<float> transformMatrix = null; // ApplyAsyncInternal(context).Result;
            ImageProjectionHelper.ProjectLayerOntoSurface(context, transformMatrix);
        }

        private float ApplyPadding2X(float val)
        {
            return (float) ((1 - 2 * PaddingPercent) * val);
        }

        private float ApplyPadding(float val)
        {
            return (float)((1 - PaddingPercent) * val);
        }
    }
}
