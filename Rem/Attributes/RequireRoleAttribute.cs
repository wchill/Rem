using System;
using System.Collections.Generic;
using System.Text;
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

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var roles = ((SocketGuildUser) context.User).Roles;
            foreach (var role in roles)
            {
                if (_roles.Contains(role.Name.ToLower()))
                {
                    return PreconditionResult.FromSuccess();
                }
            }

            return PreconditionResult.FromError("You do not have permission to use this command.");
        }
    }
}
