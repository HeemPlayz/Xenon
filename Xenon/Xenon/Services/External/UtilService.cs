#region

using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Xenon.Core;

#endregion

namespace Xenon.Services.External
{
    public static class UtilService
    {
        public static void CheckHierachy(this DiscordMember user, CommandContext ctx, string action, string action2)
        {
            CheckUserHierachy(user, ctx, action, action2);
            CheckBotHierachy(user, ctx, action, action2);
        }

        public static void CheckUserHierachy(this DiscordMember targetUser, CommandContext ctx, string action,
            string action2)
        {
            var user = ctx.Guild.GetMemberAsync(ctx.User.Id).GetAwaiter().GetResult();

            CheckHierachys(targetUser, user, action, action2);
        }

        public static void CheckBotHierachy(this DiscordMember targetUser, CommandContext ctx, string action,
            string action2)
        {
            CheckHierachys(targetUser, ctx.Guild.CurrentMember, action, action2);
        }

        public static void CheckUserHierachy(this DiscordMember targetUser, DiscordMember user, string action,
            string action2)
        {
            CheckHierachys(targetUser, user, action, action2);
        }

        private static void CheckHierachys(DiscordMember targetUser, DiscordMember user, string action, string action2)
        {
            if (user.Id == targetUser.Id) throw new HierachyException($"You cannot {action} yourself");
            if (targetUser.IsOwner)
                throw new HierachyException($"The {Formatter.InlineCode("owner")} of a server cannot be {action2}");
            if (targetUser.Hierarchy >= user.Hierarchy)
                throw new HierachyException("You have not enough permissions to do this");
        }

        public static string GetUsage(this Command command)
        {
            var descriptions = new List<string>();

            foreach (var overload in command.Overloads)
            {
                var description = $"{command.Name.ToLower()}";

                foreach (var argument in overload.Arguments)
                    if (argument.IsOptional)
                        description += $" ({argument.Name.ToLower()})";
                    else
                        description += $" <{argument.Name.ToLower()}>";

                descriptions.Add(description);
            }

            return string.Join("\n", descriptions.Select(Formatter.InlineCode));
        }

        public static int CalculateDifference(this string first, string second)
        {
            var n = first.Length;
            var m = second.Length;
            var d = new int[n + 1, m + 1];

            if (n == 0) return m;

            if (m == 0) return n;

            for (var i = 0; i <= n; d[i, 0] = i++) ;
            for (var j = 0; j <= m; d[0, j] = j++) ;

            for (var i = 1; i <= n; i++)
            for (var j = 1; j <= m; j++)
            {
                var cost = second[j - 1] == first[i - 1] ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }

            return d[n, m];
        }

        public static string ToMessage(this string message, CommandContext ctx, Userxp userxp)
        {
            var dictionary = new Dictionary<string, string>
            {
                {"%mention$", ctx.User.Mention},
                {"%user%", ctx.Member.DisplayName},
                {"%server%", ctx.Guild.Name},
                {"%level%", $"{userxp.Level}"}
            };

            foreach (var pair in dictionary)
                message = message.Replace(pair.Key, pair.Value, StringComparison.OrdinalIgnoreCase);

            return message;
        }

        public static string ToMessage(this string message, CommandContext ctx)
        {
            var dictionary = new Dictionary<string, string>
            {
                {"%mention$", ctx.User.Mention},
                {"%user%", ctx.Member.DisplayName},
                {"%server%", ctx.Guild.Name}
            };

            foreach (var pair in dictionary)
                message = message.Replace(pair.Key, pair.Value, StringComparison.OrdinalIgnoreCase);

            return message;
        }

        public static string ToMessage(this string message, MessageCreateEventArgs ctx, Userxp userxp)
        {
            var dictionary = new Dictionary<string, string>
            {
                {"%mention%", ctx.Author.Mention},
                {
                    "%user%",
                    ctx.Guild.Members.FirstOrDefault(x => x.Id == ctx.Author.Id)?.DisplayName ?? ctx.Author.Username
                },
                {"%server%", ctx.Guild.Name},
                {"%level%", $"{userxp.Level}"}
            };

            foreach (var pair in dictionary)
                message = message.Replace(pair.Key, pair.Value, StringComparison.OrdinalIgnoreCase);

            return message;
        }

        public static string ToMessage(this string message, GuildMemberAddEventArgs ctx)
        {
            var dictionary = new Dictionary<string, string>
            {
                {"%mention%", ctx.Member.Mention},
                {
                    "%user%",
                    ctx.Guild.Members.FirstOrDefault(x => x.Id == ctx.Member.Id)?.DisplayName ?? ctx.Member.Username
                },
                {"%server%", ctx.Guild.Name}
            };

            foreach (var pair in dictionary)
                message = message.Replace(pair.Key, pair.Value, StringComparison.OrdinalIgnoreCase);

            return message;
        }

        public static string ToMessage(this string message, GuildMemberRemoveEventArgs ctx)
        {
            var dictionary = new Dictionary<string, string>
            {
                {"%mention%", ctx.Member.Mention},
                {
                    "%user%",
                    ctx.Guild.Members.FirstOrDefault(x => x.Id == ctx.Member.Id)?.DisplayName ?? ctx.Member.Username
                },
                {"%server%", ctx.Guild.Name}
            };

            foreach (var pair in dictionary)
                message = message.Replace(pair.Key, pair.Value, StringComparison.OrdinalIgnoreCase);

            return message;
        }
    }
}