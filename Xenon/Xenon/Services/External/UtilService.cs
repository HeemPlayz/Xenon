#region

using System;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Xenon.Core;

#endregion

namespace Xenon.Services.External
{
    public class UtilService
    {
        public void CheckHierachy(DiscordMember user, CommandContext ctx, string action, string action2)
        {
            CheckUserHierachy(user, ctx, action, action2);
            CheckBotHierachy(user, ctx, action, action2);
        }

        public void CheckUserHierachy(DiscordMember targetUser, CommandContext ctx, string action, string action2)
        {
            var targetUserRole = targetUser.Roles.FirstOrDefault();
            var targetUserHierachy = targetUserRole == null ? 0 : targetUserRole.Position;
            var user = ctx.Guild.GetMemberAsync(ctx.User.Id).GetAwaiter().GetResult();
            var userHierachy = user.Roles.First().Position;

            CheckHierachys(targetUser, user, targetUserHierachy, userHierachy, action, action2);
        }

        public void CheckBotHierachy(DiscordMember targetUser, CommandContext ctx, string action, string action2)
        {
            var targetUserRole = targetUser.Roles.FirstOrDefault();
            var targetUserHierachy = targetUserRole == null ? 0 : targetUserRole.Position;
            var botHierachy = ctx.Guild.CurrentMember.Roles.First().Position;

            CheckHierachys(targetUser, ctx.Guild.CurrentMember, targetUserHierachy, botHierachy, action, action2);
        }

        public void CheckUserHierachy(DiscordMember targetUser, DiscordMember user, string action, string action2)
        {
            var targetUserHierachy = targetUser.Roles.First().Position;
            var userHierachy = user.Roles.First().Position;

            CheckHierachys(targetUser, user, targetUserHierachy, userHierachy, action, action);
        }

        private void CheckHierachys(DiscordMember targetUser, DiscordMember user, int targetUserHierachy,
            int userHierachy, string action, string action2)
        {
            if (user.Id == targetUser.Id) throw new HierachyException($"You cannot {action} yourself");
            if (targetUser.IsOwner)
                throw new HierachyException($"The {Formatter.InlineCode("owner")} of a server cannot be {action2}");
            if (targetUserHierachy >= userHierachy)
                throw new HierachyException("You have not enough permissions to do this");
        }

        public string GetUsage(Command command)
        {
            var description = "";

            foreach (var overload in command.Overloads)
            {
                description += $"{command.Name.ToLower()}";

                foreach (var argument in overload.Arguments)
                    if (argument.IsOptional)
                        description += $" ({argument.Name.ToLower()})";
                    else
                        description += $" <{argument.Name.ToLower()}>";
            }

            return description;
        }

        public int CalculateDifference(string first, string second)
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

        public void CheckTagHierachy(DiscordMember user, Tag tag)
        {
            if (user.IsOwner) return;

            var tagOwner = user.Guild.GetMemberAsync(tag.AuthorId).GetAwaiter().GetResult();
            if (tagOwner == null || user.Id == tagOwner.Id) return;

            if (tagOwner.Roles.First().Position >= user.Roles.First().Position)
                throw new HierachyException("You have not enough permissions to do this");
        }
    }
}