using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Rem.Bot;
using Rem.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Rem.Commands.MemeGen
{
    public class PigeonMemeGen : ModuleBase
    {
        private static readonly Font font = InitializeFont();
        private readonly BotState _botState;
        private readonly MemeTemplate _template;

        private static Font InitializeFont()
        {
            var collection = new FontCollection();
            collection.Install(FontNames.Anton, out var fontDescription);
            return collection.CreateFont(fontDescription.FontFamily, 20);
        }

        public PigeonMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("Pigeon.jpg",
                new MultiBoundingBox(
                    new TextBoundingBox(196, 181, 350, 120)
                    {
                        CenterWidth = true,
                        CenterHeight = true,
                        Brush = Brushes.Solid(Rgba32.White),
                        Pen = Pens.Solid(Rgba32.Black, 2),
                        Font = font
                    }, new ImageBoundingBox(190, 196, 375, 375)),
                new MultiBoundingBox(
                    new TextBoundingBox(738, 144, 300, 220)
                    {
                        CenterWidth = true,
                        CenterHeight = true,
                        Brush = Brushes.Solid(Rgba32.White),
                        Pen = Pens.Solid(Rgba32.Black, 2),
                        Font = font
                    }, new ImageBoundingBox(780, 148, 215, 215)),
                new TextBoundingBox(50, 621, 1100, 200)
                {
                    CenterWidth = true,
                    CenterHeight = true,
                    Brush = Brushes.Solid(Rgba32.White),
                    Pen = Pens.Solid(Rgba32.Black, 2),
                    Font = font
                });
        }

        [Command("pigeon"), Summary("Is this a pigeon?")]
        public async Task GeneratePigeonMeme(string guy, string butterfly, string bottomText)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, guy, butterfly, bottomText);
                    await Context.Channel.SendFileAsync(filename);
                    File.Delete(filename);
                }
                catch (ArgumentException e)
                {
                    await ReplyAsync(e.Message);
                }
            }
        }
    }
}
