using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rem.Commands.MemeGen
{
    public class TextBoundingBox
    {
        public PointF TopLeft { get; set; }
        public PointF TopRight { get; set; }
        public PointF BottomLeft { get; set; }
        public PointF BottomRight { get; set; }
        public float Padding { get; set; }
        public List<Rectangle> Masks { get; set; } = new List<Rectangle>();
        public IPen<Rgba32> Pen { get; set; } = null;
        public IBrush<Rgba32> Brush { get; set; } = Brushes.Solid(Rgba32.Black);
        public Font Font { get; set; } = SystemFonts.CreateFont("Arial", 20);
        public bool CenterWidth { get; set; }
        public bool CenterHeight { get; set; }
        public bool ForceNoScaling { get; set; }
        public bool PreferNoScaling { get; set; }

        public TextBoundingBox()
        {

        }

        public TextBoundingBox(float x, float y, float w, float h)
        {
            TopLeft = new PointF(x, y);
            TopRight = new PointF(x + w, y);
            BottomLeft = new PointF(x, y + h);
            BottomRight = new PointF(x + w, y + h);
        }

        public float WidthTop
        {
            get
            {
                return TopRight.X - TopLeft.X - (2 * Padding);
            }
        }

        public float WidthBottom
        {
            get
            {
                return BottomRight.X - BottomLeft.X - (2 * Padding);
            }
        }

        public float HeightLeft
        {
            get
            {
                return BottomLeft.Y - TopLeft.Y - (2 * Padding);
            }
        }

        public float HeightRight
        {
            get
            {
                return BottomRight.Y - TopRight.Y - (2 * Padding);
            }
        }

        public float MaxWidth
        {
            get
            {
                return Math.Max(WidthTop, WidthBottom);
            }
        }

        public float MaxHeight
        {
            get
            {
                return Math.Max(HeightLeft, HeightRight);
            }
        }

        public Point BoundingBoxTopLeft
        {
            get
            {
                return new Point((int) Math.Min(TopLeft.X, BottomLeft.X), (int) Math.Min(TopLeft.Y, TopRight.Y));
            }
        }

        public Point BoundingBoxBottomRight
        {
            get
            {
                return new Point((int)Math.Max(TopRight.X, BottomRight.X), (int)Math.Max(BottomLeft.Y, BottomRight.Y));
            }
        }
    }
}
