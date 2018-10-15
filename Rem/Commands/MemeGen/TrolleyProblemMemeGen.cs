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
    public class TrolleyProblemMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public TrolleyProblemMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("TrolleyProblem.png",
                new MultiBoundingBox(787, 23, 350, 120),
                new MultiBoundingBox(371, 379, 350, 120));
        }

        [Command("trolleyproblem"), Summary("Multi track drifting!")]
        public async Task GenerateTrolleyProblemMeme(string top, string bottom)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, top, bottom);
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
