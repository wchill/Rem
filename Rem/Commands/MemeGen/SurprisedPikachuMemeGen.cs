using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Rem.Bot;
using SixLabors.Fonts;
using SixLabors.Primitives;

namespace Rem.Commands.MemeGen
{
    public class SurprisedPikachuMemeGen : ModuleBase
    {
        private static readonly Font font = new Font(TextBoundingBox.DefaultFont, 40);
        private readonly BotState _botState;
        private readonly MemeTemplate _template;

        public SurprisedPikachuMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("SurprisedPikachu.png",
                new TextBoundingBox()
                {
                    TopLeft = new PointF(0, 0),
                    TopRight = new PointF(472, 0),
                    BottomLeft = new PointF(0, 390),
                    BottomRight = new PointF(472, 390),
                    CenterWidth = false,
                    PreferNoScaling = true,
                    Font = font
                });
        }

        [Command("pikachu"), Summary("Surprised Pikachu is surprised")]
        public async Task GenerateSurprisedPikachuMeme(params string[] lines)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, string.Join("\n\n", lines));
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
