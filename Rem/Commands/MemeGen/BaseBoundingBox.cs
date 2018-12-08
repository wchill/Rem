using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Rem.Commands.MemeGen
{
    public abstract class BaseBoundingBox
    {
        protected string _lastInput;

        public virtual PointF TopLeft { get; set; }
        public virtual PointF TopRight { get; set; }
        public virtual PointF BottomLeft { get; set; }
        public virtual PointF BottomRight { get; set; }
        public virtual float Padding { get; set; }
        public virtual IReadOnlyList<Rectangle> Masks { get; set; } = new List<Rectangle>();

        public float WidthTop => TopRight.X - TopLeft.X - (2 * Padding);
                                            
        public float WidthBottom => BottomRight.X - BottomLeft.X - (2 * Padding);

        public float HeightLeft => BottomLeft.Y - TopLeft.Y - (2 * Padding);

        public float HeightRight => BottomRight.Y - TopRight.Y - (2 * Padding);

        public float MaxWidth => Math.Max(WidthTop, WidthBottom);

        public float MaxHeight => Math.Max(HeightLeft, HeightRight);

        public Point BoundingBoxTopLeft => 
            new Point((int) Math.Min(TopLeft.X, BottomLeft.X), (int) Math.Min(TopLeft.Y, TopRight.Y));

        public Point BoundingBoxBottomRight => 
            new Point((int) Math.Max(TopRight.X, BottomRight.X), (int) Math.Max(BottomLeft.Y, BottomRight.Y));

        public abstract Task<bool> CanHandleAsync(string input);
        internal abstract Task<Matrix<float>> ApplyAsyncInternal(IImageProcessingContext<Rgba32> context);

        public void Apply(IImageProcessingContext<Rgba32> context)
        {
            var transformMatrix = ApplyAsyncInternal(context).Result;
            ProjectLayerOntoSurface(context, transformMatrix);
        }

        public virtual void SetInput(string input)
        {
            _lastInput = input;
        }

        protected Matrix<float> GetProjectiveTransformationMatrix(float? width = null, float? height = null)
        {
            var s = MapBasisToPoints(new PointF(0, 0), new PointF(width ?? MaxWidth, 0),
                new PointF(0, height ?? MaxHeight), new PointF(width ?? MaxWidth, height ?? MaxHeight));
            var d = MapBasisToPoints(
                TopLeft + new PointF(Padding, Padding),
                TopRight + new PointF(-Padding, Padding),
                BottomLeft + new PointF(Padding, -Padding),
                BottomRight + new PointF(-Padding, -Padding));
            var result = d.Multiply(AdjugateMatrix(s));
            var normalized = result.Divide(result[2, 2]);
            return normalized;
        }

        protected void ProjectLayerOntoSurface(IImageProcessingContext<Rgba32> context, Matrix<float> transformMatrix)
        {
            var matrix4X4 = new Matrix4x4(
                transformMatrix[0, 0], transformMatrix[1, 0], 0, transformMatrix[2, 0],
                transformMatrix[0, 1], transformMatrix[1, 1], 0, transformMatrix[2, 1],
                0, 0, 1, 0,
                transformMatrix[0, 2], transformMatrix[1, 2], 0, transformMatrix[2, 2]
            );
            context.Transform(matrix4X4, KnownResamplers.Lanczos3);
            foreach (var mask in Masks)
            {
                context.Opacity(0, mask);
            }
        }

        private static Matrix<float> AdjugateMatrix(Matrix<float> matrix)
        {
            if (matrix.RowCount != 3 || matrix.ColumnCount != 3)
            {
                throw new ArgumentException("Must provide a 3x3 matrix.");
            }

            var adj = matrix.Clone();
            adj[0, 0] = matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1];
            adj[0, 1] = matrix[0, 2] * matrix[2, 1] - matrix[0, 1] * matrix[2, 2];
            adj[0, 2] = matrix[0, 1] * matrix[1, 2] - matrix[0, 2] * matrix[1, 1];
            adj[1, 0] = matrix[1, 2] * matrix[2, 0] - matrix[1, 0] * matrix[2, 2];
            adj[1, 1] = matrix[0, 0] * matrix[2, 2] - matrix[0, 2] * matrix[2, 0];
            adj[1, 2] = matrix[0, 2] * matrix[1, 0] - matrix[0, 0] * matrix[1, 2];
            adj[2, 0] = matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0];
            adj[2, 1] = matrix[0, 1] * matrix[2, 0] - matrix[0, 0] * matrix[2, 1];
            adj[2, 2] = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

            return adj;
        }

        private static Matrix<float> MapBasisToPoints(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            var A = Matrix<float>.Build.DenseOfArray(new[,]
            {
                {p1.X, p2.X, p3.X},
                {p1.Y, p2.Y, p3.Y},
                {1, 1, 1}
            });
            var b = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.Dense(new[] {p4.X, p4.Y, 1});
            var aj = AdjugateMatrix(A);
            var v = aj.Multiply(b);
            var m = Matrix<float>.Build.DenseOfArray(new[,]
            {
                {v[0], 0, 0},
                {0, v[1], 0},
                {0, 0, v[2]}
            });
            return A.Multiply(m);
        }
    }
}
