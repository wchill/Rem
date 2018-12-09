using MemeGenerator;
using System;
using MemeGenerator.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace Tests
{
    public class TextRendererFontScalingTests
    {
        [Theory]
        [InlineData(0, 0, "test")]
        [InlineData(10, 10, "verylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaks")]
        public void ImpossibleTextConstraintsThrowsArgumentException(int width, int height, string text)
        {
            var originalFont = MemeFonts.GetDefaultFont();
            var renderer = new MemeTextRenderer(originalFont, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Center, VerticalAlignment.Center, false);
            Assert.Throws<ArgumentException>(() => renderer.GetAdjustedFont(width, height, text));
        }

        [Theory]
        [InlineData(100, 100, "")]
        [InlineData(100, 100, "test")]
        [InlineData(10000, 10000, "test")]
        [InlineData(100, 100, "verylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaks")]
        [InlineData(10000, 10000, "verylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaks")]
        [InlineData(1000, 1000, "example sentence")]
        public void TextConstraintsMetWithScalingEnabled(int width, int height, string text)
        {
            var originalFont = MemeFonts.GetDefaultFont();
            var renderer = new MemeTextRenderer(originalFont, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Center, VerticalAlignment.Center, false);
            renderer.GetAdjustedFont(width, height, text);
        }

        [Theory]
        [InlineData(100, 100, "")]
        [InlineData(100, 100, "test")]
        [InlineData(10000, 10000, "test")]
        [InlineData(10000, 10000, "verylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaks")]
        [InlineData(1000, 1000, "example sentence")]
        public void ScalingNotPerformedWithLargeCanvasWhenPreferringNoScaling(int width, int height, string text)
        {
            var originalFont = new Font(MemeFonts.GetDefaultFont(), 5);
            var renderer = new MemeTextRenderer(originalFont, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Center, VerticalAlignment.Center, true);
            var newFont = renderer.GetAdjustedFont(width, height, text);
            Assert.Equal(originalFont, newFont);
        }

        [Theory]
        [InlineData(100, 100, "verylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaksverylongstringwithverylongwordthathasnospacesorlinebreaks")]
        public void ScalingPerformedForLongTextWhenPreferringNoScaling(int width, int height, string text)
        {
            var originalFont = new Font(MemeFonts.GetDefaultFont(), 5);
            var renderer = new MemeTextRenderer(originalFont, null, Brushes.Solid(Rgba32.Black), HorizontalAlignment.Center, VerticalAlignment.Center, true);
            var newFont = renderer.GetAdjustedFont(width, height, text);
            Assert.True(originalFont.Size > newFont.Size);
        }
    }
}
