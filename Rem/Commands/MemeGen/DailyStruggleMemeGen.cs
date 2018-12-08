using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Rem.Bot;
using SixLabors.Primitives;

namespace Rem.Commands.MemeGen
{
    public class DailyStruggleMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;

        public DailyStruggleMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("DailyStruggle.png",
                new MultiBoundingBox
                {
                    TopLeft = new PointF(79, 179),
                    TopRight = new PointF(348, 129),
                    BottomLeft = new PointF(126, 312),
                    BottomRight = new PointF(401, 232)
                },
                new MultiBoundingBox
                {
                    TopLeft = new PointF(409, 121),
                    TopRight = new PointF(608, 92),
                    BottomLeft = new PointF(462, 240),
                    BottomRight = new PointF(665, 183)
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
