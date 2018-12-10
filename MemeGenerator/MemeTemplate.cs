using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace MemeGenerator
{
    public class MemeTemplate : IDisposable
    {
        public string Name { get; }
        public string Description { get; }
        public IReadOnlyList<InputField> InputFields { get; }
        private readonly Image<Rgba32> _baseImage;

        public MemeTemplate(string name, string description, Image<Rgba32> baseImage, IReadOnlyList<InputField> inputFields)
        {
            Name = name;
            Description = description;
            _baseImage = baseImage;
            InputFields = inputFields;
        }

        public Image<Rgba32> CreateMeme(params object[][] inputs)
        {
            return CreateMeme((IEnumerable<object[]>) inputs);
        }

        public Image<Rgba32> CreateMeme(IEnumerable<object[]> inputs)
        {
            var inputArray = inputs.ToArray();
            if (inputArray.Length != InputFields.Count)
            {
                throw new ArgumentException($"Input length mismatch: expected {InputFields.Count} but got {inputArray.Length} inputs");
            }

            var maskAreas = InputFields.Select(field => field.Mask);
            var meme = _baseImage.Clone();
            try
            {
                using (var mask = MaskHelper.CreateMaskFromImage(meme, maskAreas))
                {
                    meme.Mutate(ctx =>
                    {
                        for (var i = 0; i < InputFields.Count; i++)
                        {
                            var success = CreateAndApplyLayerToContext(ctx, mask, InputFields[i], inputArray[i], $"layer{i}.png");
                            if (!success)
                            {
                                throw new ArgumentException($"Input {i} could not be handled.");
                            }
                        }
                    });
                }
            }
            catch (Exception)
            {
                // Need to call dispose before throwing to prevent memory leakage
                meme.Dispose();
                throw;
            }

            return meme;
        }

        private bool CreateAndApplyLayerToContext(IImageProcessingContext<Rgba32> context, Image<Rgba32> mask, InputField inputField, IReadOnlyList<object> inputArray, string outputFilename = null)
        {
            var success = false;
            using (var layer = new Image<Rgba32>(mask.Width, mask.Height))
            {
                layer.Mutate(layerCtx =>
                {
                    // Each group of inputs corresponds to one input field
                    // Each input field may not be capable of handling all the given inputs (depending on renderers),
                    // so we try each one in succession until one succeeds (or all fail)
                    foreach (var inputObj in inputArray)
                    {
                        success = inputField.Apply(layerCtx, inputObj);
                        if (success)
                        {
                            break;
                        }
                    }

                    if (!success)
                    {
                        return;
                    }

                    // Project this layer into the correct position
                    var transformMatrix =
                        ImageProjectionHelper.CalculateProjectiveTransformationMatrix(inputField.DrawingArea.Width, inputField.DrawingArea.Height,
                            inputField.TopLeft, inputField.TopRight, inputField.BottomLeft, inputField.BottomRight);
                    ImageProjectionHelper.ProjectLayerOntoSurface(layerCtx, transformMatrix);

                    // Apply the mask layer on top to prevent overlapping the base image
                    // Draw the mask first onto this layer, then xor it back out so we will be left with only the transparent areas from the mask
                    layerCtx.DrawImage(mask, 1);
                    layerCtx.DrawImage(mask, PixelBlenderMode.Xor, 1);
                });

                // For debugging purposes
                if (outputFilename != null)
                {
                    layer.Save(outputFilename);
                }

                // Now draw this layer on top of the template
                context.DrawImage(layer, 1);
            }

            return success;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                _baseImage.Dispose();
            }
            _disposedValue = true;
        }
        
        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
