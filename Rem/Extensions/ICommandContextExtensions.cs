using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rem.Extensions
{
    public static class ICommandContextExtensions
    {
        public static IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetPreviousMessages(this ICommandContext context, int num)
        {
            return context.Channel.GetMessagesAsync(context.Message, Direction.Before, num);
        }
        public static async Task<IMessage> GetPreviousMessage(this ICommandContext context)
        {
            return (await context.Channel.GetMessagesAsync(context.Message, Direction.Before, 1).FlattenAsync()).First();
        }
    }
}
