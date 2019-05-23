using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rem.Attributes;
using Rem.Bot;
using Rem.Extensions;
using Rem.Models;
using SQLite;

namespace Rem.Commands
{
    [Name("Quote")]
    public class QuoteModule : ModuleBase
    {
        private readonly SQLiteAsyncConnection _dbContext;
        private readonly Random _random;
        private readonly BotState _botState;

        public QuoteModule(SQLiteAsyncConnection dbConnection, BotState state)
        {
            _dbContext = dbConnection;
            _random = new Random();
            _botState = state;
        }

        [RequireRole("regulars")]
        [Command("addquote"), Summary("Add a new quote to the quote database")]
        public async Task AddQuote()
        {
            var previousMessage = await Context.GetPreviousMessage();
            await AddQuote(previousMessage);
        }

        [RequireRole("regulars")]
        [Command("addquote"), Summary("Add a new quote to the quote database")]
        public async Task AddQuote(ulong messageId)
        {
            var previousMessage = await Context.Channel.GetMessageAsync(messageId);
            await AddQuote(previousMessage);
        }

        [Command("quote"), Summary("Retrieve a random quote")]
        public async Task GetQuote()
        {
            var table = _dbContext.Table<Quote>();
            var count = await table.CountAsync();
            var quote = await table.Skip(_random.Next(count)).FirstOrDefaultAsync();
            if (quote == null)
            {
                await ReplyAsync("No quotes are in the database.");
                return;
            }
            await ReplyWithQuote(quote);
        }

        [Command("quote"), Summary("Retrieve a quote by id")]
        public async Task GetQuote(int id)
        {
            try
            {
                var quote = await _dbContext.GetAsync<Quote>(id);
                await ReplyWithQuote(quote);
            }
            catch (InvalidOperationException)
            {
                await ReplyAsync("No quotes are in the database.");
            }
        }

        [Command("quote"), Summary("Retrieve a random quote by a user")]
        public async Task GetQuote(IUser user)
        {
            var table = _dbContext.Table<Quote>();
            var strId = user.Id.ToString();
            var query = table.Where(q => q.AuthorId == strId);
            var count = await query.CountAsync();
            var quote = await query.Skip(_random.Next(count)).FirstOrDefaultAsync();
            if (quote == null)
            {
                await ReplyAsync("No quotes are in the database.");
                return;
            }
            await ReplyWithQuote(quote);
        }

        private async Task AddQuote(IMessage message)
        {
            if (message == null)
            {
                await ReplyAsync("That message ID doesn't match with any message in this channel.");
                return;
            }
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                await ReplyAsync("I can't quote blank messages or rich embeds.");
                return;
            }
            var quote = new Quote
            {
                AuthorId = message.Author.Id.ToString(),
                QuotedById = Context.User.Id.ToString(),
                QuoteTime = message.Timestamp.UtcDateTime,
                QuoteString = message.Content
            };
            await _dbContext.InsertAsync(quote);
            await ReplyAsync($"Added quote (id {quote.Id}) to the database.");
        }

        private async Task ReplyWithQuote(Quote quote)
        {
            var restClient = ((DiscordSocketClient)Context.Client).Rest;
            var authorInfoTask = restClient.GetUserAsync(ulong.Parse(quote.AuthorId));
            var quoterInfoTask = restClient.GetUserAsync(ulong.Parse(quote.QuotedById));
            var users = await Task.WhenAll(authorInfoTask, quoterInfoTask);

            var authorInfo = users[0];
            var quoterInfo = users[1];

            var avatarUrl = authorInfo.GetAvatarUrl() ?? authorInfo.GetDefaultAvatarUrl();

            var builder = new EmbedBuilder();
            
            builder.WithColor(0, 255, 0);
            builder.WithDescription(quote.QuoteString);
            builder.WithFooter($"#{quote.Id} (added by {quoterInfo.Username})", quoterInfo.GetAvatarUrl());
            builder.Author = new EmbedAuthorBuilder
            {
                IconUrl = avatarUrl,
                Name = authorInfo.Username
            };
            builder.Timestamp = new DateTime(quote.QuoteTime.Ticks, DateTimeKind.Utc);

            await ReplyAsync("", embed: builder.Build());
        }
    }
}
