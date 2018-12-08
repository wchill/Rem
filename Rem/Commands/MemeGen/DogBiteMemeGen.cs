using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Rem.Bot;

namespace Rem.Commands.MemeGen
{
    public class DogBiteMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;

        public DogBiteMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("DogBite.png",
                new MultiBoundingBox(179, 519, 310, 160));
        }

        [Command("dogbite"), Summary("Does your dog bite? No, but it can hurt you in other ways.")]
        public async Task GenerateDogBiteMeme([Remainder] string dogBark)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, dogBark);
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
