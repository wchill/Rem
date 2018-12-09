using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rem.Bot;

namespace Rem.Attributes
{
    public class RequireAdminAttribute : PreconditionAttribute
    {

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Client.TokenType == TokenType.Bot)
            {
                var application = await context.Client.GetApplicationInfoAsync();
                if (context.User.Id == application.Owner.Id)
                    return PreconditionResult.FromSuccess();
            }

            var userId = context.User.Id;
            var state = (BotState) services.GetService(typeof(BotState));

            if (state.AdminList.Contains(userId))
            {
                return PreconditionResult.FromSuccess();
            }

            return PreconditionResult.FromError("You do not have permission to use this command.");
        }
    }
}
