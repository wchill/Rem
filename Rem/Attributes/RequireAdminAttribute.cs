using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rem.Bot;

namespace Rem.Attributes
{
    public class RequireAdminAttribute : PreconditionAttribute
    {
        private const string ErrorLackPermissions = "You do not have permission to use this command.";

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            if (context.Client.TokenType == TokenType.Bot)
            {
                var application = await context.Client.GetApplicationInfoAsync();

                return context.User.Id == application.Owner.Id ?
                    PreconditionResult.FromSuccess() :
                    PreconditionResult.FromError(ErrorLackPermissions);
            }

            var userId = context.User.Id;
            var state = (BotState) services.GetService(typeof(BotState));

            return state.AdminList.Contains(userId) ?
                PreconditionResult.FromSuccess() :
                PreconditionResult.FromError(ErrorLackPermissions);
        }
    }
}