using Discord.Commands;
using Rem.Bot;
using Rem.Fonts;
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
    public class KaibasDefeatMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public KaibasDefeatMemeGen(BotState state)
        {
            _botState = state;
            var fontCollection = new FontCollection();
            fontCollection.Install(FontNames.AnimeAce, out var fontDescription);
            _template = new MemeTemplate("KaibasDefeat.png",
                new MultiBoundingBox(new TextBoundingBox()
                {
                    Font = fontCollection.CreateFont(fontDescription.FontFamily, 20)
                })
                    {
                        TopLeft = new PointF(0, 213),
                        TopRight = new PointF(285, 135),
                        BottomLeft = new PointF(113, 585),
                        BottomRight = new PointF(394, 506)
                    },
                new MultiBoundingBox(new TextBoundingBox()
                {
                    Font = fontCollection.CreateFont(fontDescription.FontFamily, 20)
                })
                    {
                        TopLeft = new PointF(0, 750),
                        TopRight = new PointF(353, 685),
                        BottomLeft = new PointF(50, 1145),
                        BottomRight = new PointF(419, 1096)
                    });
        }

        [Command("exodia"), Summary("Exodia! It's not possible!")]
        public async Task GenerateKaibasDefeatMeme(string kaibasCard, string yugisCard)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, kaibasCard, yugisCard);
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
