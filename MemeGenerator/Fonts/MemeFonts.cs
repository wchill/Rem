using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.Fonts;

namespace MemeGenerator.Fonts
{
    public enum AvailableFonts
    {
        MangaTemple,
        AnimeAce,
        OpenSans,
        Anton
    }

    public static class AvailableFontsExtensions
    {
        public static string ToFileName(this AvailableFonts fontName)
        {
            switch (fontName)
            {
                case AvailableFonts.MangaTemple:
                    return "MangaTemple.ttf";
                case AvailableFonts.AnimeAce:
                    return "AnimeAce.ttf";
                case AvailableFonts.OpenSans:
                    return "OpenSans.ttf";
                case AvailableFonts.Anton:
                    return "Anton.ttf";
                default:
                    throw new ArgumentException("Not a valid font");
            }
        }
    }

    public static class MemeFonts
    {
        private const string BasePath = "Fonts";

        private static readonly Lazy<Tuple<FontCollection, Dictionary<AvailableFonts, FontDescription>>> LazyFontData =
            new Lazy<Tuple<FontCollection, Dictionary<AvailableFonts, FontDescription>>>(
                () =>
                {

                    var collection = new FontCollection();
                    var descriptions = new Dictionary<AvailableFonts, FontDescription>();
                    foreach (var font in Enum.GetValues(typeof(AvailableFonts)))
                    {
                        var fontEnum = (AvailableFonts)font;
                        collection.Install(Path.Join(BasePath, fontEnum.ToFileName()), out var fontDescription);
                        descriptions[fontEnum] = fontDescription;
                    }
                    return Tuple.Create(collection, descriptions);
                });

        public static FontCollection Collection => LazyFontData.Value.Item1;
        public static IReadOnlyDictionary<AvailableFonts, FontDescription> Descriptions => LazyFontData.Value.Item2;

        public static Font GetFont(AvailableFonts fontName, float size)
        {
            var description = Descriptions[fontName];
            return Collection.CreateFont(description.FontFamily, size);
        }

        public static Font GetDefaultFont()
        {
            return GetFont(AvailableFonts.OpenSans, 20);
        }
    }
}
