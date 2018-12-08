﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rem.Bot;

namespace Rem.Commands
{
    public class AdminModule : ModuleBase
    {
        private readonly BotState _botState;

        public AdminModule(BotState state)
        {
            _botState = state;
        }

        [RequireOwner]
        [Command("addadmin"), Summary("Add a user as an admin")]
        public async Task AddAdmin(IUser user)
        {
            if (_botState.AdminList.Contains(user.Id))
            {
                await ReplyAsync("That user is already an admin.");
                return;
            }

            _botState.AdminList.Add(user.Id);
            await ReplyAsync("Added that user as an admin.");
        }

        [RequireOwner]
        [Command("deladmin"), Summary("Remove a user as an admin")]
        public async Task RemoveAdmin(IUser user)
        {
            if (!_botState.AdminList.Contains(user.Id))
            {
                await ReplyAsync("That user is not an admin.");
                return;
            }

            _botState.AdminList.Remove(user.Id);
            await ReplyAsync("Removed that user as an admin.");
        }

        [RequireOwner]
        [Command("reloadsettings"), Summary("Reload settings")]
        public async Task ReloadSettings()
        {
            await _botState.ReloadState();
            await ReplyAsync("Settings have been reloaded.");
        }
    }
}
