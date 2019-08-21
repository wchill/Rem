using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rem.Bot;
using Rem.Models;

namespace Rem
{
    class Program
    {
#if DEBUG
        private const string EnvironmentName = "Development";
#else
        private const string EnvironmentName = "Production";
#endif
        static int Main(string[] args)
        {
            var version = File.ReadAllText("VERSION.txt");
            var configPath = Environment.GetEnvironmentVariable("REM_CONFIG_FILE");
            var dbConnectionString = Environment.GetEnvironmentVariable("REM_DB_CONNECTION_STR");

            if (configPath == null)
            {
                Console.Error.WriteLine("Config file path is required (set REM_CONFIG_FILE)");
                return 1;
            }

            if (dbConnectionString == null)
            {
                Console.Error.WriteLine("Database connection string is required (set REM_DB_CONNECTION_STR)");
                return 1;
            }

            if (!File.Exists(configPath))
            {
                Console.Error.WriteLine($"Config file does not exist at {configPath}. One will be created. Please fill in the values.");
                BotState.Initialize(version, configPath);
                return 1;
            }

            Console.WriteLine($"Running in {EnvironmentName}");
            Console.WriteLine($"Bot build version: {version}");
            Console.WriteLine($"Loading bot config from {configPath}");

            var dbContextOptionsBuilder = new DbContextOptionsBuilder<BotContext>();
            dbContextOptionsBuilder.UseSqlite(dbConnectionString);

            using (var dbContext = new BotContext(dbContextOptionsBuilder.Options))
            {
                dbContext.Database.Migrate();
            }

            var source = new CancellationTokenSource();

            var bot = new DiscordBot(version, configPath, dbConnectionString);
            var botTask = bot.Start();
            Task.WaitAll(botTask, Task.Delay(-1, source.Token));

            return 0;
        }
    }
}
