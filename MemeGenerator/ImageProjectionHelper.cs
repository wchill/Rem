using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using SixLabors.Primitives;

namespace MemeGenerator
{
    public static class ImageProjectionHelper
    {
        public static Matrix4x4 CalculateProjectiveTransformationMatrix(int width, int height, Point newTopLeft, Point newTopRight, Point newBottomLeft, Point newBottomRight)
        {
            var s = MapBasisToPoints(0, 0, width, 0, 0, height, width, height);
            var d = MapBasisToPoints(newTopLeft, newTopRight, newBottomLeft, newBottomRight);
            var result = d.Multiply(AdjugateMatrix(s));
            var normalized = result.Divide(result[2, 2]);
            return new Matrix4x4(
                (float)normalized[0, 0], (float)normalized[1, 0], 0, (float)normalized[2, 0],
                (float)normalized[0, 1], (float)normalized[1, 1], 0, (float)normalized[2, 1],
                0, 0, 1, 0,
                (float)normalized[0, 2], (float)normalized[1, 2], 0, (float)normalized[2, 2]
            );
        }
        private static Matrix<double> AdjugateMatrix(Matrix<double> matrix)
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

        private static Matrix<double> MapBasisToPoints(Point p1, Point p2, Point p3, Point p4)
        {
            return MapBasisToPoints(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }

        private static Matrix<double> MapBasisToPoints(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            var A = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {x1, x2, x3},
                {y1, y2, y3},
                {1, 1, 1}
            });
            var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(new double[] { x4, y4, 1 });
            var aj = AdjugateMatrix(A);
            var v = aj.Multiply(b);
            var m = Matrix<double>.Build.DenseOfArray(new[,]
            {
                {v[0], 0, 0 },
                {0, v[1], 0 },
                {0, 0, v[2] }
            });
            return A.Multiply(m);
        }
    }
}