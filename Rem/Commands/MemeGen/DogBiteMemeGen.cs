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
    public class DogBiteMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly TextMemeTemplate _template;
        public DogBiteMemeGen(BotState state)
        {
            _botState = state;
            _template = new TextMemeTemplate("DogBite.png",
                new TextBoundingBox(179, 519, 310, 160)
                {
                    CenterWidth = true,
                    CenterHeight = true
                });
        }

        [Command("dogbite"), Summary("Does your dog bite? No, but it can hurt you in other ways.")]
        public async Task GenerateDogBiteMeme([Remainder]string dogBark)
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
