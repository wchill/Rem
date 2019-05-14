using MemeGenerator;
using MemeGenerator.Fonts;
using MemeGenerator.JsonConverters;
using Newtonsoft.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests
{
    public class MemeTemplateSerializationTests
    {
        [Fact]
        public void RoundtripTemplateSerialization()
        {
            var template = GetMemeTemplate();
            var inputParams = new[] { "test" };

            var firstImage = template.CreateMeme(inputParams);

            var converters = new List<JsonConverter>
            {
                new FontJsonConverter(),
                new PenJsonConverter()
            };
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = converters
            };

            var serialized = JsonConvert.SerializeObject(template, settings);
            var deserialized = JsonConvert.DeserializeObject<MemeTemplate>(serialized, settings);

            var secondImage = deserialized.CreateMeme(inputParams);

            Assert.Equal(firstImage.Width, secondImage.Width);
            Assert.Equal(firstImage.Height, secondImage.Height);

            for (var y = 0; y < firstImage.Height; y++)
            {
                for (var x = 0; x < firstImage.Width; x++)
                {
                    Assert.Equal(firstImage[x, y], secondImage[x, y]);
                }
            }
        }

        private static MemeTemplate GetMemeTemplate()
        {
            var font = MemeFonts.GetDefaultFont();
            var textRenderer = new TextInputRenderer(font, Pens.Dash<Rgba32>(Rgba32.Black, 1), Brushes.Solid(Rgba32.Black), HorizontalAlignment.Center, VerticalAlignment.Center, false);
            var disappointment = new InputFieldBuilder()
                .WithName("Baby")
                .WithVertices(new Point(115, 720), new Point(584, 720), new Point(115, 884), new Point(584, 884))
                .WithRenderer(textRenderer)
                .WithPadding(0.03)
                .Build();
            return new MemeTemplateBuilder("TestData/Disappointment.png")
                .WithName("Disappointment")
                .WithDescription("Free disappointment")
                .WithInputField(disappointment)
                .Build();
        }
    }
}
