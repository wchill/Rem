using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using Rem.Bot;
using Discord;

namespace Rem.Commands
{
    public class MemeHelpModule : ModuleBase
    {
        private readonly BotState _botState;
        private readonly CommandService _commandService;

        public MemeHelpModule(BotState state, CommandService commandService)
        {
            _botState = state;
            _commandService = commandService;
        }

        [Command("meme"), Summary("Get list of generatable memes")]
        public async Task GetAllMemeCommands()
        {
            var commands = _commandService.Commands.Where(ci => ci.Module.Name.Contains("MemeGen")).OrderBy(ci => ci.Name).ToArray();

            var builder = new EmbedBuilder();

            builder.WithTitle("List of meme commands");
            builder.WithColor(0, 255, 0);

            var half = commands.Length / 2;

            builder.AddField("Meme list", string.Join("\n", commands.Take(half).Select(ci => ci.Name)), true);
            builder.AddField("Meme list", string.Join("\n", commands.Skip(half).Select(ci => ci.Name)), true);

            await ReplyAsync("", embed: builder.Build());
        }

        [Command("meme"), Summary("Get help for a meme")]
        public async Task GetMemeCommand(string meme)
        {
            var command = _commandService.Commands.Where(ci => ci.Module.Name.Contains("MemeGen") && ci.Name == meme).FirstOrDefault();
            if (command == null)
            {
                await ReplyAsync("That meme doesn't exist.");
                return;
            }

            var builder = new EmbedBuilder();

            builder.WithTitle(meme);
            builder.WithColor(0, 255, 0);
            builder.WithDescription(command.Summary);
            builder.AddField("Usage", $"{_botState.Prefix}{command.Name} <{string.Join("> <", command.Parameters.Select(p => p.Name).ToArray())}>");

            await ReplyAsync("", embed: builder.Build());
        }
    }
}
