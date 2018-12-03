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
    public class KomiPaperMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public KomiPaperMemeGen(BotState state)
        {
            _botState = state;
            var fontCollection = new FontCollection();
            fontCollection.Install(FontNames.AnimeAce, out var fontDescription);
            _template = new MemeTemplate("KomiPaper.png",
                new MultiBoundingBox(new TextBoundingBox()
                {
                    /*
                    TopLeft = new PointF(245, 479),
                    TopRight = new PointF(708, 558),
                    BottomLeft = new PointF(168, 728),
                    BottomRight = new PointF(632, 810),
                    */
                    CenterHeight = false,
                    CenterWidth = false,
                    Font = fontCollection.CreateFont(fontDescription.FontFamily, 20)
                }, new ImageBoundingBox()
                {
                    /*
                    TopLeft = new PointF(245, 479),
                    TopRight = new PointF(708, 558),
                    BottomLeft = new PointF(94, 988),
                    BottomRight = new PointF(557, 1067),
                    */
                    LandscapeScalingMode = ImageScalingMode.FitWithLetterbox,
                    PortraitScalingMode = ImageScalingMode.FitWithLetterbox
                })
                {
                    TopLeft = new PointF(245, 479),
                    TopRight = new PointF(708, 558),
                    BottomLeft = new PointF(94, 988),
                    BottomRight = new PointF(557, 1067),
                    Padding = 10,
                    Masks = new List<Rectangle>
                    {
                        new Rectangle(0, 631, 186, 446),
                        new Rectangle(666, 552, 50, 150),
                        new Rectangle(0, 1065, 557, 205)
                    }
                });
        }

        [Command("komi"), Summary("Komi-san reads something and gets excited!")]
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
