using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Rem.Commands.MemeGen
{
    public class MultiBoundingBox : BaseBoundingBox
    {
        private readonly BaseBoundingBox[] _boundingBoxes;
        private BaseBoundingBox _currentSelectedBox;

        public override PointF TopLeft
        {
            get => GetBoundingBox().TopLeft;
            set
            {
                foreach (var box in _boundingBoxes)
                {
                    box.TopLeft = value;
                }
            }
        }

        public override PointF TopRight
        {
            get => GetBoundingBox().TopRight;
            set
            {
                foreach (var box in _boundingBoxes)
                {
                    box.TopRight = value;
                }
            }
        }

        public override PointF BottomLeft
        {
            get => GetBoundingBox().BottomLeft;
            set
            {
                foreach (var box in _boundingBoxes)
                {
                    box.BottomLeft = value;
                }
            }
        }

        public override PointF BottomRight
        {
            get => GetBoundingBox().BottomRight;
            set
            {
                foreach (var box in _boundingBoxes)
                {
                    box.BottomRight = value;
                }
            }
        }

        public override float Padding
        {
            get => GetBoundingBox().Padding;
            set
            {
                foreach (var box in _boundingBoxes)
                {
                    box.Padding = value;
                }
            }
        }

        public override IReadOnlyList<Rectangle> Masks
        {
            get => GetBoundingBox().Masks;
            set
            {
                foreach (var box in _boundingBoxes)
                {
                    box.Masks = value;
                }
            }
        }

        public MultiBoundingBox() : this(0, 0, 0, 0)
        {
        }

        public MultiBoundingBox(float x, float y, float w, float h) : this(x, y, w, h, new TextBoundingBox(),
            new ImageBoundingBox())
        {
        }

        public MultiBoundingBox(params BaseBoundingBox[] boundingBoxes)
        {
            _boundingBoxes = boundingBoxes;
        }

        public MultiBoundingBox(float x, float y, float w, float h, params BaseBoundingBox[] boundingBoxes)
        {
            _boundingBoxes = boundingBoxes;

            var tl = new PointF(x, y);
            var tr = new PointF(x + w, y);
            var bl = new PointF(x, y + h);
            var br = new PointF(x + w, y + h);

            foreach (var box in _boundingBoxes)
            {
                box.TopLeft = tl;
                box.TopRight = tr;
                box.BottomLeft = bl;
                box.BottomRight = br;
            }
        }

        public override async Task<bool> CanHandleAsync(string input)
        {
            var result = await Task.WhenAll(_boundingBoxes.Select(async (box) => await box.CanHandleAsync(input)));
            return result.Any(b => b);
        }

        internal override async Task<Matrix<float>> ApplyAsyncInternal(IImageProcessingContext<Rgba32> context)
        {
            return await GetBoundingBox().ApplyAsyncInternal(context);
        }

        public override void SetInput(string input)
        {
            _lastInput = input;
            foreach (var box in _boundingBoxes)
            {
                if (!box.CanHandleAsync(input).Result) continue;
                box.SetInput(input);
                _currentSelectedBox = box;
            }
        }

        private BaseBoundingBox GetBoundingBox()
        {
            return _currentSelectedBox ?? _boundingBoxes[0];
        }
    }
}
