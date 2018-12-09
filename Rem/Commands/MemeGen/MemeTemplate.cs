using Discord.Commands;
using Rem.Bot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rem.Commands.MemeGen
{
    public class MemeTemplate
    {
        private static readonly string ImageFolder = @"Images/MemeTemplates/";
        private readonly string _imagePath;
        private readonly BaseBoundingBox[] _boundingBoxes;

        public MemeTemplate(string filename, params BaseBoundingBox[] boundingBoxes)
        {
            _imagePath = ImageFolder + filename;
            _boundingBoxes = boundingBoxes;
        }

        public async Task<string> GenerateImage(ICommandContext context, params string[] inputs)
        {
            if (inputs.Length != _boundingBoxes.Length)
            {
                throw new ArgumentException($"Expected {_boundingBoxes.Length} fields but got {inputs.Length}.");
            }

            if (inputs.Length == 1 && inputs[0].Length > 2 && inputs[0].First() == '\"' && inputs[0].Last() == '\"')
            {
                inputs[0] = inputs[0].Substring(1, inputs[0].Length - 2);
            }

            using (var img = Image.Load(_imagePath))
            {
                for (var i = 0; i < inputs.Length; i++)
                {
                    inputs[i] = await context.Guild.ResolveIds(inputs[i]);
                }

                using (var img2 = img.Clone())
                {
                    for (var i = 0; i < _boundingBoxes.Length; i++)
                    {
                        img2.Mutate(ctx => ctx.CreateLayerFromBoundingBox(_boundingBoxes[i], inputs[i]));
                    }
                    var filename = Guid.NewGuid().ToString() + ".png";
                    img2.Save(filename);
                    return filename;
                }
            }
        }
    }
}
