using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Rem.Attributes
{
    public class RequireRoleAttribute : PreconditionAttribute
    {
        private readonly HashSet<string> _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = new HashSet<string>();

            foreach (var role in roles)
            {
                _roles.Add(role.ToLower());
            }
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var roles = ((SocketGuildUser) context.User).Roles;
            //does this really just need any match?
            return Task.FromResult(roles.Any(role => _roles.Contains(role.Name.ToLower())) ?
                PreconditionResult.FromSuccess() :
                PreconditionResult.FromError("You do not have permission to use this command."));
        }
    }
}