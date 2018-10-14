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
    public class DailyStruggleMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly TextMemeTemplate _template;
        public DailyStruggleMemeGen(BotState state)
        {
            _botState = state;
            _template = new TextMemeTemplate("DailyStruggle.png",
                new TextBoundingBox
                {
                    TopLeft = new PointF(79, 179),
                    TopRight = new PointF(348, 129),
                    BottomLeft = new PointF(126, 312),
                    BottomRight = new PointF(401, 232),
                    CenterWidth = true,
                    CenterHeight = true
                },
                new TextBoundingBox
                {
                    TopLeft = new PointF(409, 121),
                    TopRight = new PointF(608, 92),
                    BottomLeft = new PointF(462, 240),
                    BottomRight = new PointF(665, 183),
                    CenterWidth = true,
                    CenterHeight = true
                });
        }

        [Command("dailystruggle"), Summary("Two buttons, which one to choose?")]
        public async Task GenerateDailyStruggleMeme(string leftButton, string rightButton)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, leftButton, rightButton);
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
