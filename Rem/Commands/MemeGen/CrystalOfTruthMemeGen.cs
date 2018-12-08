using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Rem.Bot;

namespace Rem.Commands.MemeGen
{
    public class CrystalOfTruthMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;

        public CrystalOfTruthMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("CrystalOfTruth.png",
                new TextBoundingBox(70, 530, 380, 80));
        }

        [Command("crystaloftruth"), Summary("At long last I have found it, the crystal which utters only truth!")]
        public async Task GenerateCrystalOfTruthMeme([Remainder] string truth)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, truth);
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
