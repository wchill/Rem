using Discord.Commands;
using Rem.Bot;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rem.Commands.MemeGen
{
    public class KomiPaperMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly TextMemeTemplate _template;
        public KomiPaperMemeGen(BotState state)
        {
            _botState = state;
            _template = new TextMemeTemplate("KomiPaper.png", 
                new TextBoundingBox
                {
                    TopLeft = new PointF(122, 238),
                    TopRight = new PointF(354, 279),
                    BottomLeft = new PointF(84, 364),
                    BottomRight = new PointF(316, 405),
                    Padding = 10,
                    Masks =
                    {
                        new Rectangle(73, 316, 19, 49),
                        new Rectangle(333, 277, 21, 72)
                    }
                });
        }

        [Command("komi"), Summary("Komi-san reads a paper")]
        public async Task GenerateKomiPaperMeme([Remainder] string text)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, text);
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
