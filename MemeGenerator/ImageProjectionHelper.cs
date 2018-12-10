using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace MemeGenerator
{
    public static class ImageProjectionHelper
    {
        public static Matrix<float> CalculateProjectiveTransformationMatrix(Rectangle drawingArea, PointF newTopLeft, PointF newTopRight, PointF newBottomLeft, PointF newBottomRight)
        {
            var x = drawingArea.X;
            var y = drawingArea.Y;
            var w = drawingArea.Width;
            var h = drawingArea.Height;

            var s = MapBasisToPoints(
                new PointF(x, y),
                new PointF(x + w, y),
                new PointF(x, y + h),
                new PointF(x + w, y + h));
            var d = MapBasisToPoints(newTopLeft, newTopRight, newBottomLeft, newBottomRight);
            var result = d.Multiply(AdjugateMatrix(s));
            var normalized = result.Divide(result[2, 2]);
            return normalized;
        }
        public static void ProjectLayerOntoSurface(IImageProcessingContext<Rgba32> context, Matrix<float> transformMatrix)
        {
            var matrix4X4 = new Matrix4x4(
                transformMatrix[0, 0], transformMatrix[1, 0], 0, transformMatrix[2, 0],
                transformMatrix[0, 1], transformMatrix[1, 1], 0, transformMatrix[2, 1],
                0, 0, 1, 0,
                transformMatrix[0, 2], transformMatrix[1, 2], 0, transformMatrix[2, 2]
            );
            context.Transform(matrix4X4, KnownResamplers.Lanczos3);
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
            var b = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.Dense(new[] { p4.X, p4.Y, 1 });
            var aj = AdjugateMatrix(A);
            var v = aj.Multiply(b);
            var m = Matrix<float>.Build.DenseOfArray(new[,]
            {
                {v[0], 0, 0 },
                {0, v[1], 0 },
                {0, 0, v[2] }
            });
            return A.Multiply(m);
        }
    }
}