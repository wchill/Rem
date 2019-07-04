using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Rem.Attributes;
using Rem.Bot;
using Rem.Models;
using Image = Rem.Models.Image;

namespace Rem.Commands
{
    [Name("Headpats")]
    public class PatModule : ModuleBase
    {
        private readonly BotContext _dbContext;
        private readonly Random _random;
        private readonly BotState _botState;

        private static readonly Dictionary<ImageType, Tuple<string, string>> TemplateStrings = new Dictionary<ImageType, Tuple<string, string>>
        {
            {ImageType.Pat, Tuple.Create("{0} had their head patted.", "{0} had their head patted by {1}.")},
            {ImageType.Hug, Tuple.Create("{0}, (>^_^)> <(^.^<)", "{0}, (>^_^)> <(^.^<) from {1}")},
            {ImageType.Cuddle, Tuple.Create("{0} was cuddled.", "{0} was cuddled by {1}.")}
        };

        public PatModule(BotContext dbConnection, BotState state)
        {
            _dbContext = dbConnection;
            _random = new Random();
            _botState = state;
        }

        [Command("pat"), Summary("Headpats a user (or yourself if not specified)")]
        public async Task Pat([Summary("The person to headpat")] IUser user = null, [Remainder] string text = null)
        {
            await HandleImageCommand(ImageType.Pat, user);
        }

        [Command("hug"), Summary("Hug a user (or yourself if not specified)")]
        public async Task Hug([Summary("The person to hug")] IUser user = null, [Remainder] string text = null)
        {
            await HandleImageCommand(ImageType.Hug, user);
        }

        [Command("cuddle"), Summary("Cuddle a user (or yourself if not specified)")]
        public async Task Cuddle([Summary("The person to cuddle")] IUser user = null, [Remainder] string text = null)
        {
            await HandleImageCommand(ImageType.Cuddle, user);
        }

        private async Task HandleImageCommand(ImageType type, IUser user)
        {
            var imageCount = await _dbContext.Images.CountAsync(image => image.Type == type);
            if (imageCount == 0)
            {
                await ReplyAsync($"There are no {type.ToFriendlyString()} images in the database, add some with -addimage {type.ToFriendlyString()} <url>");
                return;
            }
            var imageUrl = await _dbContext.Images.Where(image => image.Type == type).OrderBy(p => p.Id).Skip(_random.Next(imageCount)).FirstAsync();

            var userInfo = user ?? Context.User;
            var isSelf = userInfo == Context.User;

            var builder = new EmbedBuilder();
            var templateStrings = TemplateStrings[type];
            var title = isSelf ? string.Format(templateStrings.Item1, userInfo.Username) : string.Format(templateStrings.Item2, userInfo.Username, Context.User.Username);

            builder.WithTitle(title);
            builder.WithImageUrl(imageUrl.Url);
            builder.WithColor(218, 185, 255);
            await ReplyAsync("", embed: builder.Build());
            _botState.PatCount += 1;
        }

        [RequireRole("regulars")]
        [Command("addimage"), Summary("Add a new image to the image database")]
        public async Task AddNewImage([Summary("The image type")] string type, [Summary("The image URL")] string url)
        {
            try
            {
                ImageType t;
                switch (type)
                {
                    case "pat":
                        t = ImageType.Pat;
                        break;
                    case "cuddle":
                        t = ImageType.Cuddle;
                        break;
                    case "hug":
                        t = ImageType.Hug;
                        break;
                    default:
                        await ReplyAsync("That is not a valid image type.");
                        return;
                }

                _dbContext.Add(new Image {Url = url, Type = t});
                await _dbContext.SaveChangesAsync();

                var count = await _dbContext.Images.CountAsync(image => image.Type == t);
                await ReplyAsync($"Added image to the database ({count} total).");
            }
            catch (Exception)
            {
                await ReplyAsync($"There was an error adding the image.");
            }
        }

        [RequireRole("regulars")]
        [Command("delimage"), Summary("Remove an image from the image database")]
        public async Task DeleteImage([Summary("The image URL")] string url)
        {
            try
            {
                _dbContext.Images.Remove(_dbContext.Images.First(x => x.Url == url));
                await _dbContext.SaveChangesAsync();

                await ReplyAsync("Deleted image from the database.");
            }
            catch (Exception)
            {
                await ReplyAsync("There was an error deleting the image.");
            }
        }
    }
}
