using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MemeGenerator
{
    public class MemeTemplate : IDisposable
    {
        public string Name { get; }
        public string Description { get; }
        public IReadOnlyList<InputField> InputFields { get; }
        public string ImagePath { get; }
        private readonly Lazy<Image<Rgba32>> BaseImage;

        public MemeTemplate(string name, string description, string imagePath, IReadOnlyList<InputField> inputFields)
        {
            Name = name;
            Description = description;
            ImagePath = imagePath;
            InputFields = inputFields;

            BaseImage = new Lazy<Image<Rgba32>>(() => Image.Load(ImagePath));
        }

        private Image<Rgba32> CreateMask()
        {
            var maskAreas = InputFields.Select(field => field.Mask);
            return MaskHelper.CreateMaskFromImage(BaseImage.Value, maskAreas);
        }

        private Image<Rgba32> CreateLayer(Image<Rgba32> mask, InputField inputField, IReadOnlyList<object> inputArray)
        {
            var layer = new Image<Rgba32>(inputField.MaxWidth, inputField.MaxHeight);
            try
            {
                layer.Mutate(layerCtx =>
                {
                    // Each group of inputs corresponds to one input field
                    // Each input field may not be capable of handling all the given inputs (depending on renderers),
                    // so we try each one in succession until one succeeds (or all fail)
                    if (!inputArray.Any(inputObj => inputField.Apply(layerCtx, inputObj)))
                    {
                        throw new ArgumentException("Unable to handle input.");
                    }

                    // Project this layer into the correct position
                    var transformMatrix =
                        ImageProjectionHelper.CalculateProjectiveTransformationMatrix(inputField.MaxWidth, inputField.MaxHeight,
                            inputField.TopLeft, inputField.TopRight, inputField.BottomLeft,
                            inputField.BottomRight);
                    layerCtx.Transform(new ProjectiveTransformBuilder().AppendMatrix(transformMatrix), KnownResamplers.Lanczos3);

                    // Apply the mask layer on top to prevent overlapping the base image
                    // Draw the mask first onto this layer, then xor it back out so we will be left with only the transparent areas from the mask
                    layerCtx.DrawImage(mask, 1);
                    layerCtx.DrawImage(mask, PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.Xor, 1);
                });

                return layer;
            }
            catch (Exception)
            {
                layer.Dispose();
                throw;
            }
        }

        public IReadOnlyList<Image<Rgba32>> CreateLayers(IReadOnlyList<object[]> inputs)
        {
            if (inputs.Count != InputFields.Count)
            {
                throw new ArgumentException($"Input length mismatch: expected {InputFields.Count} but got {inputs.Count} inputs");
            }

            var allLayers = new List<Image<Rgba32>>();
            var mask = CreateMask();
            allLayers.Add(mask);
            allLayers.AddRange(InputFields.Select((t, i) => CreateLayer(mask, t, inputs[i])));

            return allLayers;
        }

        public Image<Rgba32> CreateMeme(params object[][] inputs)
        {
            return CreateMeme((IReadOnlyList<object[]>) inputs);
        }

        public Image<Rgba32> CreateMeme(IReadOnlyList<object[]> inputs)
        {
            var allLayers = CreateLayers(inputs);
            try
            {
                return CreateMeme(allLayers);
            }
            finally
            {
                allLayers.Dispose();
            }
        }

        public Image<Rgba32> CreateMeme(IReadOnlyList<Image<Rgba32>> allLayers)
        {
            var meme = BaseImage.Value.Clone();
            try
            {
                meme.Mutate(ctx =>
                {
                    var maskLayer = allLayers.First();
                    var otherLayers = allLayers.Skip(1);
                    foreach (var layer in otherLayers)
                    {
                        ctx.DrawImage(layer, 1);
                    }
                });
            }
            catch (Exception)
            {
                // Need to call dispose before throwing to prevent memory leakage
                meme.Dispose();
                throw;
            }
            return meme;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue || !BaseImage.IsValueCreated)
            {
                return;
            }

            if (disposing)
            {
                BaseImage.Value.Dispose();
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
