using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rem.Bot;

namespace Rem
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.WaitAll(new Program().MainAsync());
        }

        public async Task MainAsync()
        {
            var bot = new DiscordBot("settings.json");
            await bot.Start();
            await Task.Delay(-1);
        }
    }
}
