using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
            await _dbContext.InsertAsync(new Quote
            {
                AuthorId = previousMessage.Author.Id.ToString(),
                QuotedById = Context.User.Id.ToString(),
                QuoteTime = previousMessage.Timestamp.UtcDateTime,
                QuoteString = previousMessage.Content
            });
            await ReplyAsync("Added quote to the database.");
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

        private async Task ReplyWithQuote(Quote quote)
        {
            var authorInfoTask = Context.Client.GetUserAsync(ulong.Parse(quote.AuthorId));
            var quoterInfoTask = Context.Client.GetUserAsync(ulong.Parse(quote.QuotedById));
            var users = await Task.WhenAll(authorInfoTask, quoterInfoTask);

            var authorInfo = users[0];
            var quoterInfo = users[1];

            var builder = new EmbedBuilder();

            builder.WithTitle("Quote");
            builder.WithColor(0, 255, 0);
            builder.WithDescription(quote.QuoteString);
            builder.WithThumbnailUrl(authorInfo.GetAvatarUrl());
            builder.WithFooter($"#{quote.Id} by {quoterInfo.Username} on {quote.QuoteTime} UTC", quoterInfo.GetAvatarUrl());

            await ReplyAsync("", embed: builder.Build());
        }
    }
}
