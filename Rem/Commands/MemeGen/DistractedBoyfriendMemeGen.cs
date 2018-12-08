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
    public class DistractedBoyfriendMemeGen : ModuleBase
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

        public DistractedBoyfriendMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("DistractedBoyfriend.jpg",
                new MultiBoundingBox(
                    new TextBoundingBox(1338, 823, 500, 260)
                    {
                        Brush = Brushes.Solid(Rgba32.White),
                        Pen = Pens.Solid(Rgba32.Black, 2),
                        Font = font
                    }, new ImageBoundingBox(1338, 703, 500, 500)),
                new MultiBoundingBox(
                    new TextBoundingBox(1874, 829, 440, 260)
                    {
                        Brush = Brushes.Solid(Rgba32.White),
                        Pen = Pens.Solid(Rgba32.Black, 2),
                        Font = font
                    }, new ImageBoundingBox(1844, 689, 500, 500)),
                new MultiBoundingBox(
                    new TextBoundingBox(487, 1016, 500, 260)
                    {
                        Brush = Brushes.Solid(Rgba32.White),
                        Pen = Pens.Solid(Rgba32.Black, 2),
                        Font = font
                    }, new ImageBoundingBox(487, 876, 500, 500)));
        }

        [Command("distractedbf"),
         Summary("Disloyal man walking with his girlfriend and looking amazed at another seductive girl")]
        public async Task GenerateDistractedBfMeme(string guy, string girlfriend, string otherGirl)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, guy, girlfriend, otherGirl);
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
