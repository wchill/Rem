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
            templates["dailystruggle"] = LoadDailyStruggleMemeTemplate();
            templates["coma"] = LoadComaMemeTemplate();
            templates["crystaloftruth"] = LoadCrystalOfTruthMemeTemplate();
            templates["distractedbf"] = LoadDistractedBoyfriendMemeTemplate();
            templates["disappointment"] = LoadDisappointmentMemeTemplate();
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

        public static MemeTemplate LoadDailyStruggleMemeTemplate()
        {
            var imageRenderer = GetDefaultImageInputRenderer();
            var textRenderer = GetDefaultTextInputRenderer();

            var leftInputField = new InputFieldBuilder()
                .WithName("Left button", "Left button label")
                .WithVertices(new Point(52, 165), new Point(358, 109), new Point(115, 327), new Point(423, 239))
                .WithRenderer(imageRenderer)
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();

            var rightInputField = new InputFieldBuilder()
                .WithName("Right button", "Right button label")
                .WithVertices(new Point(385, 105), new Point(615, 77), new Point(454, 246), new Point(677, 183))
                .WithRenderer(imageRenderer)
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();

            return new MemeTemplateBuilder("Images/MemeTemplates/DailyStruggle.png")
                .WithName("Daily Struggle")
                .WithDescription("My daily life is a struggle when I have to pick a button")
                .WithInputField(leftInputField)
                .WithInputField(rightInputField)
                .Build();
        }

        public static MemeTemplate LoadComaMemeTemplate()
        {
            var font = new Font(MemeFonts.GetDefaultFont(), 36);
            var textRenderer = new TextInputRenderer(font, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Center, VerticalAlignment.Center, true);

            var nurseText = new InputFieldBuilder()
                .WithName("Nurse", "Sir, you've been in a coma for...")
                .WithVertices(new Point(145, 73), new Point(715, 73), new Point(145, 147), new Point(715, 147))
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();

            var patientText = new InputFieldBuilder()
                .WithName("Patient response", "Oh boy, I can't wait to...")
                .WithVertices(new Point(158, 150), new Point(725, 150), new Point(158, 260), new Point(725, 260))
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();

            return new MemeTemplateBuilder("Images/MemeTemplates/Coma.jpg")
                .WithName("Coma")
                .WithDescription("Sir, you've been in a coma for...")
                .WithInputField(nurseText)
                .WithInputField(patientText)
                .Build();
        }

        public static MemeTemplate LoadCrystalOfTruthMemeTemplate()
        {
            var textRenderer = GetDefaultTextInputRenderer();

            var truthText = new InputFieldBuilder()
                .WithName("Truth", "The truth")
                .WithVertices(new Point(70, 530), new Point(450, 530), new Point(70, 610), new Point(450, 610))
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();

            return new MemeTemplateBuilder("Images/MemeTemplates/CrystalOfTruth.png")
                .WithName("Crystal of Truth")
                .WithDescription("At long last I have found it, the crystal which utters only truth!")
                .WithInputField(truthText)
                .Build();
        }

        public static MemeTemplate LoadDisappointmentMemeTemplate()
        {
            var textRenderer = GetDefaultTextInputRenderer();

            var disappointment = new InputFieldBuilder()
                .WithName("Baby")
                .WithVertices(new Point(115, 720), new Point(584, 720), new Point(115, 884), new Point(584, 884))
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();

            return new MemeTemplateBuilder("Images/MemeTemplates/Disappointment.png")
                .WithName("Disappointment")
                .WithDescription("Free disappointment")
                .WithInputField(disappointment)
                .Build();
        }

        public static MemeTemplate LoadDistractedBoyfriendMemeTemplate()
        {
            var imageRenderer = GetDefaultImageInputRenderer();
            var font = MemeFonts.GetFont(AvailableFonts.Anton);
            var textRenderer = new TextInputRenderer(font, Pens.Solid(Rgba32.Black, 2), Brushes.Solid(Rgba32.White), HorizontalAlignment.Center, VerticalAlignment.Center, false);

            var boyfriend = new InputFieldBuilder()
                .WithName("Boyfriend")
                .WithVertices(new Point(1338, 823), new Point(1838, 823), new Point(1338, 1083), new Point(1838, 1083))
                .WithRenderer(imageRenderer)
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();

            var girlfriend = new InputFieldBuilder()
                .WithName("Girlfriend")
                .WithVertices(new Point(1874, 829), new Point(2314, 829), new Point(1874, 1089), new Point(2314, 1089))
                .WithRenderer(imageRenderer)
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();

            var otherGirl = new InputFieldBuilder()
                .WithName("The other girl")
                .WithVertices(new Point(487, 1016), new Point(987, 1016), new Point(487, 1276), new Point(987, 1276))
                .WithRenderer(imageRenderer)
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();

            return new MemeTemplateBuilder("Images/MemeTemplates/DistractedBoyfriend.jpg")
                .WithName("Distracted Boyfriend")
                .WithDescription("Disloyal man walking with his girlfriend and looking amazed at another seductive girl")
                .WithInputField(boyfriend)
                .WithInputField(girlfriend)
                .WithInputField(otherGirl)
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
