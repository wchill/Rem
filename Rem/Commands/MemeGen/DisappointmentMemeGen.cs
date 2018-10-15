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
    public class DisappointmentMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public DisappointmentMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("Disappointment.png",
                new TextBoundingBox(115, 720, 469, 164)
                {
                    CenterWidth = true,
                    CenterHeight = true
                });
        }

        [Command("disappointment"), Summary("Free disappointment")]
        public async Task GenerateDisappointmentMeme([Remainder] string babyResponse)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, babyResponse);
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
