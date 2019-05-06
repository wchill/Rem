using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Rem.Attributes;

namespace Rem.Extensions
{
    public static class MentionExtensions
    {
        // https://discordapp.com/developers/docs/reference#message-formatting
        public static string Mention(this IUser user)
        {
            return $"<@{user.Id}>";
        }
        public static string Mention(this IRole role)
        {
            return $"<@&{role.Id}>";
        }
        public static string Mention(this IChannel channel)
        {
            return $"<#{channel.Id}>";
        }
    }
}