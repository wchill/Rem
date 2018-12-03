using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rem.Bot;

namespace Rem
{
    class Program
    {
        static int Main(string[] args)
        {
            return new Program().MainAsync(args).Result;
        }

        public async Task<int> MainAsync(string[] args)
        {
            string configPath = null;
            string dbPath = null;
            if (args.Length >= 2)
            {
                configPath = args[0];
                dbPath = args[1];
            }
            else
            {
                configPath = Environment.GetEnvironmentVariable("REM_CONFIG_FILE");
                dbPath = Environment.GetEnvironmentVariable("REM_SQLITE_DB");
            }

            if (configPath == null || dbPath == null)
            {
                Console.Error.WriteLine($"Config file and DB path are required (either as args or as environment variables)");
                return 1;
            }

            Console.WriteLine($"Using {configPath} for config file");
            Console.WriteLine($"Using {dbPath} for SQLite database");
            
            if (!File.Exists(configPath))
            {
                Console.Error.WriteLine("Config file does not exist. One will be created. Please fill in the values.");
                BotState.Initialize(configPath);
                return 1;
            }
            if (!File.Exists(dbPath))
            {
                Console.Error.WriteLine("SQLite database does not exist. One will be created.");
            }

            var bot = new DiscordBot(configPath, dbPath);
            await bot.Start();
            await Task.Delay(-1);

            return 0;
        }
    }
}
