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
    public class AlarmClockMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public AlarmClockMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("AlarmClock.png",
                new TextBoundingBox(1755, 190, 790, 120)
                {
                    CenterWidth = true,
                    CenterHeight = true
                });
        }

        [Command("alarmclock"), Summary("Alarm Clock V2, Designed to Wake You Up!")]
        public async Task GenerateAlarmClockMeme([Remainder] string alarm)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, alarm);
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
