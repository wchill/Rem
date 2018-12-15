using MemeGenerator;
using SixLabors.Primitives;
using System;
using System.Linq;
using System.Collections.Generic;


namespace Rem.Utilities.MemeModels
{
    class InputField
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Padding { get; set; }
        public double[][] Vertices { get; set; }
        public double[][] Mask { get; set; }
        public object[] Renderers { get; set; }

        internal MemeGenerator.InputField Convert()
        {
            if (Vertices == null || Vertices.Length != 4)
                throw new Exception("Corners were not specified");
            if (Renderers == null || Renderers.Length == 0)
                throw new Exception("At least one renderer must be specified");

            return new MemeGenerator.InputField(Name, Description,
                Point(Vertices, 0), Point(Vertices, 1), Point(Vertices, 2), Point(Vertices, 3),
                Padding, GetRenderers(), GetMask());
        }

        private IReadOnlyList<Point> GetMask()
        {
            var mask = Mask?.Select(a => new Point((int)a[0], (int)a[1])).ToList();
            if (mask != null)
            {
                if (mask.First() != mask.Last())
                {
                    var newList = new List<Point>(mask) { mask.First() };
                    mask = newList;
                }
            }
            else
            {
                mask = new List<Point>
                {
                    Point(Vertices, 0), Point(Vertices, 1), Point(Vertices, 2), Point(Vertices, 3), Point(Vertices, 0)
                };
            }
            return mask;
        }

        private IReadOnlyList<IInputRenderer> GetRenderers()
        {
            return Renderers.Select(r =>
            {
                switch (r)
                {
                    case TextRenderer tr: return tr.GetRenderer();
                    case ImageRenderer ir: return ir.GetRenderer();;
                    default: return (IInputRenderer)null;
                }
            })
            .Where(r => r != null).ToList();
        }

        static Point Point(double[][] arr, int idx)
        {
            if (arr[idx].Length != 2)
                throw new ArgumentException("array must have a length of 2");

            return new Point((int)arr[idx][0], (int)arr[idx][1]);
        }
    }
}
