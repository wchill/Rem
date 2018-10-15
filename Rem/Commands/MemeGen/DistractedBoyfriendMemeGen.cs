using Discord.Commands;
using Rem.Bot;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rem.Commands.MemeGen
{
    public class DistractedBoyfriendMemeGen : ModuleBase
    {
        private static readonly Font font = SystemFonts.CreateFont("Impact", 20, FontStyle.Bold);
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public DistractedBoyfriendMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("DistractedBoyfriend.jpg",
                new TextBoundingBox(1338, 823, 500, 260)
                {
                    CenterWidth = true,
                    CenterHeight = true,
                    Brush = Brushes.Solid(Rgba32.White),
                    Pen = Pens.Solid(Rgba32.Black, 2),
                    Font = font
                },
                new TextBoundingBox(1874, 829, 440, 260)
                {
                    CenterWidth = true,
                    CenterHeight = true,
                    Brush = Brushes.Solid(Rgba32.White),
                    Pen = Pens.Solid(Rgba32.Black, 2),
                    Font = font
                },
                new TextBoundingBox(487, 1016, 500, 260)
                {
                    CenterWidth = true,
                    CenterHeight = true,
                    Brush = Brushes.Solid(Rgba32.White),
                    Pen = Pens.Solid(Rgba32.Black, 2),
                    Font = font
                });
        }

        [Command("distractedbf"), Summary("Disloyal man walking with his girlfriend and looking amazed at another seductive girl")]
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
