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
    public class PigeonMemeGen : ModuleBase
    {
        private static readonly Font font = SystemFonts.CreateFont("Impact", 20, FontStyle.Bold);
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public PigeonMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("Pigeon.jpg",
                new TextBoundingBox(196, 181, 350, 120)
                {
                    CenterWidth = true,
                    CenterHeight = true,
                    Brush = Brushes.Solid(Rgba32.White),
                    Pen = Pens.Solid(Rgba32.Black, 2),
                    Font = font
                },
                new TextBoundingBox(738, 144, 300, 220)
                {
                    CenterWidth = true,
                    CenterHeight = true,
                    Brush = Brushes.Solid(Rgba32.White),
                    Pen = Pens.Solid(Rgba32.Black, 2),
                    Font = font
                },
                new TextBoundingBox(277, 741, 694, 80)
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
