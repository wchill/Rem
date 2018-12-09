using Discord.Commands;
using Rem.Bot;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Rem.Commands.MemeGen
{
    public class GruPlanMemeGen : ModuleBase
    {
        private readonly BotState _botState;
        private readonly MemeTemplate _template;
        public GruPlanMemeGen(BotState state)
        {
            _botState = state;
            _template = new MemeTemplate("GruPlan.png",
                new MultiBoundingBox(new TextBoundingBox()
                {
                    CenterHeight = false
                }, new ImageBoundingBox())
                {
                    TopLeft = new PointF(425, 114),
                    TopRight = new PointF(702, 94),
                    BottomLeft = new PointF(403, 450),
                    BottomRight = new PointF(680, 430),
                    Padding = 5,
                    Masks = new List<Rectangle>
                    {
                        new Rectangle(691, 85, 20, 400)
                    }
                },
                new MultiBoundingBox(new TextBoundingBox()
                {
                    CenterHeight = false
                }, new ImageBoundingBox())
                {
                    TopLeft = new PointF(1118, 114),
                    TopRight = new PointF(1395, 94),
                    BottomLeft = new PointF(1096, 450),
                    BottomRight = new PointF(1373, 430),
                    Padding = 5,
                    Masks = new List<Rectangle>
                    {
                        new Rectangle(1384, 85, 20, 400)
                    }
                },
                new MultiBoundingBox(new TextBoundingBox()
                {
                    CenterHeight = false
                }, new ImageBoundingBox())
                {
                    TopLeft = new PointF(425, 562),
                    TopRight = new PointF(702, 542),
                    BottomLeft = new PointF(403, 898),
                    BottomRight = new PointF(680, 878),
                    Padding = 5,
                    Masks = new List<Rectangle>
                    {
                        new Rectangle(691, 533, 20, 400)
                    }
                },
                new MultiBoundingBox(new TextBoundingBox()
                {
                    CenterHeight = false
                }, new ImageBoundingBox())
                {
                    TopLeft = new PointF(1118, 562),
                    TopRight = new PointF(1395, 542),
                    BottomLeft = new PointF(1096, 898),
                    BottomRight = new PointF(1373, 878),
                    Padding = 5,
                    Masks = new List<Rectangle>
                    {
                        new Rectangle(1384, 533, 20, 400)
                    }
                });
        }

        [Command("gruplan"), Summary("I sit on the toilet!")]
        public async Task GenerateGruPlanMeme(string step1, string step2, string step3)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var filename = await _template.GenerateImage(Context, step1, step2, step3, step3);
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
