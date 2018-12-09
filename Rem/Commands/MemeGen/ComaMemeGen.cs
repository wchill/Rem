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
    public class ComaMemeGen : ModuleBase
    {
        private static readonly Font font = new Font(TextBoundingBox.DefaultFont, 36);
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public ComaMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("Coma.jpg",
                new TextBoundingBox(145, 73, 570, 74)
                {
                    PreferNoScaling = true,
                    CenterHeight = false,
                    CenterWidth = false,
                    Font = font
                },
                new TextBoundingBox(158, 150, 567, 110)
                {
                    PreferNoScaling = true,
                    CenterHeight = false,
                    CenterWidth = false,
                    Font = font
                });
        }

        [Command("coma"), Summary("Sir, you've been in a coma for...")]
        public async Task GenerateComaMeme(string time, string patientResponse)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, time, patientResponse);
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
