using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Rem.Bot;

namespace Rem.Commands.MemeGen
{
    public class ScrollOfTruthMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;

        public ScrollOfTruthMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("ScrollOfTruth.jpg",
                new MultiBoundingBox(241, 719, 220, 280));
        }

        [Command("scrolloftruth"), Summary("I've finally found it... after 15 years... the scroll of truth!")]
        public async Task GenerateScrollOfTruthMeme([Remainder] string scrollText)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, scrollText);
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
