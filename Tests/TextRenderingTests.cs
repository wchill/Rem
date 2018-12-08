using MemeGenerator;
using System;
using MemeGenerator.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace Tests
{
    public class TextRenderingTests
    {
        [Theory]
        [InlineData(0, 0, "test")]
        [InlineData(10, 10, "verylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaks")]
        public void ImpossibleTextConstraintsThrowsArgumentException(float width, float height, string text)
        {
            var originalFont = MemeFonts.GetDefaultFont();
            var renderer = new MemeTextRenderer(originalFont, null, Brushes.Solid(Rgba32.Black), true, true, false);
            Assert.Throws<ArgumentException>(() => renderer.GetAdjustedFont(width, height, text));
        }

        [Theory]
        [InlineData(100, 100, "test")]
        [InlineData(10000, 10000, "test")]
        [InlineData(100, 100, "verylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaks")]
        [InlineData(10000, 10000, "verylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaks")]
        public void TextConstraintsMet(float width, float height, string text)
        {
            var originalFont = MemeFonts.GetDefaultFont();
            var renderer = new MemeTextRenderer(originalFont, null, Brushes.Solid(Rgba32.Black), true, true, false);
            renderer.GetAdjustedFont(width, height, text);
        }
    }
}
