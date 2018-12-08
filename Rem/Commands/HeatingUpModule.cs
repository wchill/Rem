using System;
using System.Threading.Tasks;
using Discord.Commands;
using Rem.Bot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Rem.Commands
{
    public class HeatingUpModule : ModuleBase
    {
        private readonly BotState _botState;
        private static Image<Rgba32>[] _digits;
        private static Image<Rgba32> _heatingUpImg;

        public HeatingUpModule(BotState state)
        {
            _botState = state;
            if (_digits == null)
            {
                _digits = new Image<Rgba32>[10];

                for (var i = 0; i < 10; i++)
                {
                    _digits[i] = Image.Load(@"Images/HeatingUp/" + $"{i}.png");
                }
            }

            if (_heatingUpImg == null)
            {
                _heatingUpImg = Image.Load(@"Images/HeatingUp/heatingup.png");
            }
        }

        [Command("heatingup"), Summary("Whoa, this thing's finally heating up for the Xth time.")]
        public async Task HeatingUp()
        {
            _botState.BribeCount += 1;
            using (Context.Channel.EnterTypingState())
            {
                using (var img = GenerateImage(_botState.BribeCount))
                {
                    img.Save("output.png");
                    await Context.Channel.SendFileAsync("output.png");
                }
            }
        }

        [Command("coolingdown"), Summary("Whoa, this thing's cooling down.")]
        public async Task CoolingDown()
        {
            _botState.BribeCount -= 1;
            using (Context.Channel.EnterTypingState())
            {
                using (var img = GenerateImage(_botState.BribeCount))
                {
                    img.Mutate(i => i.Rotate(RotateMode.Rotate180));
                    img.Save("output.png");
                    await Context.Channel.SendFileAsync("output.png");
                }
            }
        }

        private Image<Rgba32> GenerateImage(int num)
        {
            if (num >= 100)
            {
                num = 99;
            }

            var tens = num / 10;
            var ones = num % 10;

            var maxWidth = 54;
            var maxHeight = 54;

            var tensImg = _digits[tens];
            var onesImg = _digits[ones];

            var combinedWidth = tensImg.Width + onesImg.Width + 2;
            var scaleFactorWidth = maxWidth / (double) combinedWidth;
            var scaleFactorHeight = maxHeight / (double) Math.Max(tensImg.Height, onesImg.Height);

            var scaleFactor = Math.Min(scaleFactorHeight, scaleFactorWidth);
            var finalHeight = (int) (Math.Max(tensImg.Height, onesImg.Height) * scaleFactor);
            var finalWidth = (int) (combinedWidth * scaleFactor);

            var img = _heatingUpImg.Clone();
            var tensScaled = tensImg.Clone();
            tensScaled.Mutate(i =>
                i.Resize((int) (tensScaled.Width * scaleFactor), (int) (tensScaled.Height * scaleFactor)));
            var onesScaled = onesImg.Clone();
            onesScaled.Mutate(i =>
                i.Resize((int) (onesScaled.Width * scaleFactor), (int) (onesScaled.Height * scaleFactor)));

            var startX = 507 + (maxWidth - finalWidth) / 2;
            var startX2 = 507 + maxWidth - ((maxWidth - finalWidth) / 2 + onesScaled.Width);

            img.Mutate(i => i.DrawImage(tensScaled, 1, new Point(startX, 182 + (maxHeight - tensScaled.Height) / 2)));
            img.Mutate(i => i.DrawImage(onesScaled, 1, new Point(startX2, 182 + (maxHeight - onesScaled.Height) / 2)));

            tensScaled.Dispose();
            onesScaled.Dispose();

            return img;
        }
    }
}
