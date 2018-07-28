#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using MoreLinq;
using Xenon.Core;
using Xenon.Services.External;

#endregion

namespace Xenon.Modules
{
    public class LogModule : CommandModule
    {
        private readonly DatabaseService _databaseService;
        private readonly LogService _logService;

        public LogModule(DatabaseService databaseService, LogService logService)
        {
            _databaseService = databaseService;
            _logService = logService;
        }

        [Command("reason")]
        [Description("Lets you set a reason for a specific log object")]
        [RequireGuild]
        public async Task ReasonAsync(CommandContext ctx, ulong id, [RemainingText] string reason)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder();
            if (server.ModLog.TryGetValue(id, out var logItem))
            {
                if (logItem.ResponsibleUserId == ctx.User.Id)
                {
                    embed.WithTitle("Reason updated")
                        .WithDescription(
                            $"Updated the reason for {Formatter.InlineCode($"{id}")} to {Formatter.InlineCode(reason)}")
                        .WithColor(DiscordColor.Purple);
                    logItem.Reason = reason;
                    server.ModLog[id] = logItem;
                    _databaseService.AddOrUpdateObject(server, ctx.Guild.Id);
                }
                else
                {
                    embed.WithTitle("Missing Permissions")
                        .WithDescription("This log item doesn't belong to you")
                        .WithColor(DiscordColor.Purple);
                }
            }
            else
            {
                embed.WithTitle("Not found")
                    .WithDescription($"Couldn't find a log object with the id {Formatter.InlineCode($"{id}")}")
                    .WithColor(DiscordColor.Purple);
            }

            await ctx.RespondAsync(embed: embed);
        }
    }

    [Group("log")]
    [RequireGuild]
    [RequirePermissions(Permissions.ViewAuditLog)]
    public class GetLogModule : CommandModule
    {
        private readonly DatabaseService _databaseService;
        private readonly InteractivityExtension _interactivityExtension;

        public GetLogModule(DatabaseService databaseService, InteractivityExtension interactivityExtension)
        {
            _databaseService = databaseService;
            _interactivityExtension = interactivityExtension;
        }

        [GroupCommand]
        public async Task SendLogAsync(CommandContext ctx)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            if (!server.ModLog.Any())
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("No log entrys")
                    .WithDescription("There are no log entrys on this server yet")
                    .WithColor(DiscordColor.Purple);
                await ctx.RespondAsync(embed: embed);
                return;
            }

            var pages = new List<Page>();
            var pageIndex = 1;
            var seperatedObjects = server.ModLog.Values.Batch(5);
            foreach (var objects in seperatedObjects)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Log history")
                    .WithColor(DiscordColor.Purple)
                    .WithDescription(string.Join("", string.Join("\n", objects.Select(x =>
                    {
                        var user = ctx.Guild.Members.FirstOrDefault(y => y.Id == x.UserId);

                        var actionUser = ctx.Guild.Members.FirstOrDefault(y => y.Id == x.ResponsibleUserId);

                        return
                            $"❯ {Formatter.Bold($"{x.LogId}.")} {x.ActionType} ❯ {(actionUser == null ? "invalid user" : actionUser.Mention)} ⇒ {(user == null ? "invalid user" : user.Mention)} ❯ Reason ❯ {x.Reason}";
                    }))))
                    .WithFooter($"Page {pageIndex}/{seperatedObjects.Count()}");
                pages.Add(new Page {Embed = embed});
                pageIndex++;
            }

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages, null,
                TimeoutBehaviour.DeleteMessage, new PaginationEmojis(ctx.Client));
        }

        [GroupCommand]
        public async Task SendLogAsync(CommandContext ctx, DiscordMember user)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            var userLog = server.ModLog.Values.Where(x => x.ResponsibleUserId == user.Id || x.UserId == user.Id);
            if (!userLog.Any())
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("No log entrys")
                    .WithDescription($"There are no log entrys from {user.Mention} yet")
                    .WithColor(DiscordColor.Purple);
                await ctx.RespondAsync(embed: embed);
                return;
            }

            var pages = new List<Page>();
            var pageIndex = 1;
            var seperatedObjects = userLog.Batch(5);
            foreach (var objects in seperatedObjects)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Log history")
                    .WithColor(DiscordColor.Purple)
                    .WithDescription(string.Join("", string.Join("\n", objects.Select(x =>
                    {
                        var targetUser = ctx.Guild.Members.FirstOrDefault(y => y.Id == x.UserId);

                        var actionUser = ctx.Guild.Members.FirstOrDefault(y => y.Id == x.ResponsibleUserId);

                        return
                            $"❯ {Formatter.Bold($"{x.LogId}.")} {x.ActionType} ❯ {(actionUser == null ? "invalid user" : actionUser.Mention)} ⇒ {(targetUser == null ? "invalid user" : targetUser.Mention)} ❯ Reason ❯ {x.Reason ?? "none"}";
                    }))))
                    .WithFooter($"Page {pageIndex}/{seperatedObjects.Count()}");
                pages.Add(new Page {Embed = embed});
                pageIndex++;
            }

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages, null,
                TimeoutBehaviour.DeleteMessage, new PaginationEmojis(ctx.Client));
        }

        [GroupCommand]
        public async Task SendLogAsync(CommandContext ctx, [RemainingText] string category)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            if (!server.ModLog.Any())
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("No log entrys")
                    .WithDescription($"There are no log entrys on this server yet")
                    .WithColor(DiscordColor.Purple);
                await ctx.RespondAsync(embed: embed);
                return;
            }

            if (!Enum.TryParse(typeof(AuditLogActionType), category, true, out var specificCategory))
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Category no found")
                    .WithDescription($"There is no category with the name {Formatter.InlineCode(category)}")
                    .WithColor(DiscordColor.Purple);
                await ctx.RespondAsync(embed: embed);
                return;
            }

            var categoryLog = server.ModLog.Values.Where(x => x.ActionType == (AuditLogActionType) specificCategory);

            if (!categoryLog.Any())
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("No log entrys")
                    .WithDescription(
                        $"There are no log entrys with the category {specificCategory.ToString().ToLower()} yet")
                    .WithColor(DiscordColor.Purple);
                await ctx.RespondAsync(embed: embed);
                return;
            }

            var pages = new List<Page>();
            var pageIndex = 1;
            var seperatedObjects = categoryLog.Batch(5);
            foreach (var objects in seperatedObjects)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Log history")
                    .WithColor(DiscordColor.Purple)
                    .WithDescription(string.Join("", string.Join("\n", objects.Select(x =>
                    {
                        var targetUser = ctx.Guild.Members.FirstOrDefault(y => y.Id == x.UserId);

                        var actionUser = ctx.Guild.Members.FirstOrDefault(y => y.Id == x.ResponsibleUserId);

                        return
                            $"❯ {Formatter.Bold($"{x.LogId}.")} {x.ActionType} ❯ {(actionUser == null ? "invalid user" : actionUser.Mention)} ⇒ {(targetUser == null ? "invalid user" : targetUser.Mention)} ❯ Reason ❯ {x.Reason ?? "none"}";
                    }))))
                    .WithFooter($"Page {pageIndex}/{seperatedObjects.Count()}");
                pages.Add(new Page {Embed = embed});
                pageIndex++;
            }


            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages, null,
                TimeoutBehaviour.DeleteMessage, new PaginationEmojis(ctx.Client));
        }
    }
}