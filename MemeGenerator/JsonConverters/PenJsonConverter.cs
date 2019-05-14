using MemeGenerator.Fonts;
using Newtonsoft.Json;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemeGenerator.JsonConverters
{
    public class PenJsonConverter : JsonConverter<Pen<Rgba32>>
    {
        class PenInfo
        {
            public IBrush<Rgba32> strokeFill;
            public float strokeWidth;
            public float[] strokePattern;
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override void WriteJson(JsonWriter writer, Pen<Rgba32> value, JsonSerializer serializer)
        {
            var props = new PenInfo
            {
                strokeFill = value.StrokeFill,
                strokeWidth = value.StrokeWidth,
                strokePattern = value.StrokePattern.ToArray()
            };
            serializer.Serialize(writer, props);
        }

        public override Pen<Rgba32> ReadJson(JsonReader reader, Type objectType, Pen<Rgba32> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var props = serializer.Deserialize<PenInfo>(reader);
            return new Pen<Rgba32>(props.strokeFill, props.strokeWidth, props.strokePattern);
        }
    }
}
