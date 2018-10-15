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
    public class LightBulbMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public LightBulbMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("LightBulb.png",
                new TextBoundingBox(112, 767, 335, 126)
                {
                    CenterWidth = true,
                    CenterHeight = true
                });
        }

        [Command("lightbulb"), Summary("I've invented a stress powered light bulb!")]
        public async Task GenerateLightBulbMeme([Remainder] string whisper)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, whisper);
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
