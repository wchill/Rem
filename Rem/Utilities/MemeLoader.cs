using System;
using System.Collections.Generic;
using System.Text;
using MemeGenerator;
using MemeGenerator.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Rem.Utilities
{
    public static class MemeLoader
    {
        // TODO: Implement deserialization from file instead of doing it all in code

        public static Dictionary<string, MemeTemplate> LoadAllTemplates()
        {
            var templates = new Dictionary<string, MemeTemplate>();
            templates["komi"] = LoadKomiMemeTemplate();
            return templates;
        }

        public static MemeTemplate LoadKomiMemeTemplate()
        {
            var imageRenderer = GetDefaultImageInputRenderer();

            var font = MemeFonts.GetFont(AvailableFonts.AnimeAce);
            var textRenderer = new TextInputRenderer(font, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Left, VerticalAlignment.Top, false);

            var inputField = new InputFieldBuilder()
                .WithName("Paper", "Whatever Komi's looking at on the paper")
                .WithVertices(new Point(245, 479), new Point(707, 563), new Point(48, 980), new Point(557, 1067))
                .WithRenderer(imageRenderer)
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .WithMask(new Point(245, 479), new Point(665, 556), new Point(665, 699), new Point(555, 1066), new Point(184, 1068), new Point(184, 633))
                .Build();

            return new MemeTemplateBuilder("Images/MemeTemplates/KomiPaper.png")
                .WithName("Komi")
                .WithDescription("Komi-san reads something and gets excited!")
                .WithInputField(inputField)
                .Build();
        }

        public static ImageInputRenderer GetDefaultImageInputRenderer()
        {
            return new ImageInputRenderer(ImageScalingMode.FitWithLetterbox, new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 8
            });
        }

        public static TextInputRenderer GetDefaultTextInputRenderer()
        {
            var font = MemeFonts.GetDefaultFont();
            return new TextInputRenderer(font, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Center, VerticalAlignment.Center, false);
        }
    }
}
