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
    public class HardToSwallowPillsMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly TextMemeTemplate _template;
        public HardToSwallowPillsMemeGen(BotState state)
        {
            _botState = state;
            _template = new TextMemeTemplate("Pills.jpg", new TextBoundingBox(192, 887, 437, 285)
            {
                CenterWidth = true,
                CenterHeight = true
            });
        }

        [Command("pills"), Summary("Hard to swallow pills")]
        public async Task GenerateKomiPaperMeme([Remainder] string pills)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, pills);
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
