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

namespace Rem.Commands
{
    [Name("Meta")]
    public class MetaModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly BotState _botState;

        public MetaModule(DiscordSocketClient client, CommandService commandService, BotState state)
        {
            _client = client;
            _commandService = commandService;
            _botState = state;
        }
        
        [Command("info"), Summary("Get bot info")]
        public async Task ListInfo()
        {
            var builder = new EmbedBuilder();

            builder.WithTitle("Bot info");
            builder.WithColor(0, 255, 0);

            builder.AddField("User", $"{_client.CurrentUser} ({_client.CurrentUser.Id})");
            builder.AddField("Version", _botState.Version);
            builder.AddField("Modules installed", GetNonHiddenModules().Count());
            builder.AddField("Commands available", FilterDuplicateCommandNames(GetNonHiddenCommands()).Count());
            builder.AddField("Uptime", GetUptimeString());
            builder.AddField("Latency", $"{_client.Latency}ms");
            builder.AddField("Source code", "https://github.com/wchill/Rem");

            await ReplyAsync("", embed: builder.Build());
        }

        private string GetUptimeString()
        {
            var uptime = DateTime.UtcNow - _botState.ConnectionTime;
            var output = new List<string>();
            if (uptime.Days > 0)
            {
                output.Add($"{uptime.Days}d");
            }
            if (uptime.Hours > 0)
            {
                output.Add($"{uptime.Hours}h");
            }
            if (uptime.Minutes > 0)
            {
                output.Add($"{uptime.Minutes}m");
            }
            output.Add($"{uptime.Seconds}.{uptime.Milliseconds}s");
            return string.Join(' ', output);
        }

        [Command("modules"), Summary("Get available modules")]
        public async Task ListModules()
        {
            var builder = new EmbedBuilder();

            builder.WithTitle("List of modules");
            builder.WithColor(0, 255, 0);

            var moduleNames = GetNonHiddenModules().Select(module => module.Name).ToArray();
            builder.AddField("Module list", string.Join("\n", moduleNames));

            await ReplyAsync("", embed: builder.Build());
        }

        [Command("commands"), Summary("Get available commands")]
        [Alias("help")]
        public async Task ListCommands()
        {
            var builder = new EmbedBuilder();

            builder.WithTitle("List of commands");
            builder.WithFooter($"Type {_botState.Prefix}help <command> to get help for an individual command");
            builder.WithColor(0, 255, 0);
            
            foreach (var module in GetNonHiddenModules())
            {
                var commandNames = FilterDuplicateCommandNames(GetNonHiddenModuleCommands(module));
                if (commandNames.Any())
                {
                    builder.AddField(module.Name, string.Join("\n", commandNames), true);
                }
            }

            await ReplyAsync("", embed: builder.Build());
        }

        [Command("help"), Summary("Get help for a command")]
        public async Task GetHelp(string commandName, int page = 1)
        {
            var builder = new EmbedBuilder();

            var commandInfos = GetNonHiddenCommands().Where(command => command.Aliases.Contains(commandName)).ToArray();
            if (!commandInfos.Any())
            {
                await ReplyAsync($"Command \"{commandName}\" does not exist.");
                return;
            }

            if (page < 1 || page > commandInfos.Length)
            {
                await ReplyAsync("Invalid page specified.");
                return;
            }

            var commandInfo = commandInfos[page - 1];

            builder.WithTitle(commandInfo.Name);
            builder.WithDescription(commandInfo.Summary);
            builder.WithColor(0, 255, 0);
            builder.WithFooter($"Page {page} of {commandInfos.Length} - use {_botState.Prefix}help {commandName} <page> to view a different page");

            builder.AddField("Aliases", string.Join(' ', commandInfo.Aliases));

            if (commandInfo.Parameters.Any())
            {
                var parameterNames = commandInfo.Parameters.Select(parameter => $"<{parameter.Name}>").ToArray();
                builder.AddField("Usage", $"{_botState.Prefix}{commandName} {string.Join(' ', parameterNames)}");
            }
            else
            {
                builder.AddField("Usage", $"{_botState.Prefix}{commandName}");
            }

            await ReplyAsync("", embed: builder.Build());
        }

        private IEnumerable<ModuleInfo> GetNonHiddenModules()
        {
            // TODO: Take into account permissions of calling user
            var modules = _commandService.Modules;
            return modules.Where(module => !module.Attributes.Any(attribute => attribute is HiddenModuleAttribute));
        }

        private IEnumerable<CommandInfo> GetNonHiddenModuleCommands(ModuleInfo module)
        {
            // TODO: Take into account permissions of calling user
            return module.Commands.Where(command => {
                if (command.Attributes.Any(attribute => attribute is HiddenCommandAttribute))
                {
                    return false;
                }
                if (command.Preconditions.Any(precondition => precondition is RequireOwnerAttribute))
                {
                    return false;
                }
                return true;
            });
        }

        private IEnumerable<CommandInfo> GetNonHiddenCommands()
        {
            var modules = GetNonHiddenModules();
            return modules.SelectMany(module => GetNonHiddenModuleCommands(module));
        }

        private HashSet<string> FilterDuplicateCommandNames(IEnumerable<CommandInfo> commands)
        {
            return new HashSet<string>(commands.Select(command => command.Aliases.First()));
        }
    }
}
