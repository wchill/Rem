using MemeGenerator.Fonts;
using Newtonsoft.Json;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemeGenerator.JsonConverters
{
    public class FontJsonConverter : JsonConverter<Font>
    {
        class FontInfo
        {
            public string fontName;
            public float fontSize;
            public FontStyle fontStyle;
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override void WriteJson(JsonWriter writer, Font value, JsonSerializer serializer)
        {
            var props = new FontInfo
            {
                fontName = value.Family.Name,
                fontSize = value.Size
            };

            if (value.Bold && value.Italic)
            {
                props.fontStyle = FontStyle.BoldItalic;
            }
            else if (value.Bold)
            {
                props.fontStyle = FontStyle.Bold;
            }
            else if (value.Italic)
            {
                props.fontStyle = FontStyle.Italic;
            }
            else
            {
                props.fontStyle = FontStyle.Regular;
            }

            serializer.Serialize(writer, props);
        }

        public override Font ReadJson(JsonReader reader, Type objectType, Font existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var props = serializer.Deserialize<FontInfo>(reader);
            return MemeFonts.GetFont(props.fontName, props.fontSize, props.fontStyle);
        }
    }
}
