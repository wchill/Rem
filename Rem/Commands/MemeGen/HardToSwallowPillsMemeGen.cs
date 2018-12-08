﻿using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Rem.Bot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rem.Commands.MemeGen
{
    public class HardToSwallowPillsMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;

        public HardToSwallowPillsMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("Pills.jpg",
                new MultiBoundingBox(192, 887, 437, 285,
                    new TextBoundingBox(),
                    new ImageBoundingBox()
                    {
                        GraphicsOptions = new GraphicsOptions(true)
                        {
                            AntialiasSubpixelDepth = 8,
                            BlenderMode = PixelBlenderMode.Normal,
                            BlendPercentage = 0.5f
                        }
                    }));
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
