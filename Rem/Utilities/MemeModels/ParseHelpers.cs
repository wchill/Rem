using System;
using System.Globalization;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Rem.Utilities.MemeModels
{
    static class ParseHelpers
    {
        public static Pen<Rgba32> GetPen(this uint? hex, int width)
            => hex.HasValue ? Pens.Solid(new Rgba32(hex.Value), width) : null;

        public static SolidBrush<Rgba32> GetBrush(this uint? hex)
            => hex.HasValue ? Brushes.Solid(new Rgba32(hex.Value)) : null;

        public static uint? Hex(this string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return null;

            if (hex.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase) ||
                hex.StartsWith("&H", StringComparison.CurrentCultureIgnoreCase))
            {
                hex = hex.Substring(2);
            }

            if (!uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var hexValue))
                return null;
            return hexValue;
        }
    }
}
