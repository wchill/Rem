using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Rem.Bot;
using SQLite;

namespace Rem.Commands
{
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
    }
}
