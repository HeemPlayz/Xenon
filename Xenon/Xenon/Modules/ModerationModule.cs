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
using Sparrow.Platform.Posix.macOS;
using Xenon.Core;
using Xenon.Services.External;

#endregion

namespace Xenon.Modules
{
    public class ModerationModule : CommandModule
    {
        private readonly DatabaseService _databaseService;
        private readonly LogService _logService;
        private readonly UtilService _utilService;

        public ModerationModule(UtilService utilService, LogService logService, DatabaseService databaseService)
        {
            _utilService = utilService;
            _logService = logService;
            _databaseService = databaseService;
        }

        [Command("ban")]
        [Description("Bans a user")]
        [RequirePermissions(Permissions.BanMembers)]
        [RequireGuild]
        public async Task BanAsync(CommandContext ctx,
            DiscordMember user, [RemainingText] string reason = null)
        {
            _utilService.CheckHierachy(user, ctx, "ban", "banned");

            var embed = new DiscordEmbedBuilder()
                .WithTitle("You got banned")
                .WithDescription(
                    $"Server ❯ {ctx.Guild.Name}\nResponsible User ❯ {ctx.User.Mention}\nReason ❯ {reason ?? "none"}")
                .WithColor(DiscordColor.Purple);

            await user.SendMessageAsync(embed: embed);

            await user.BanAsync(reason: reason);

            var auditLog = (await ctx.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.Ban)).First();

            var logId = _logService.CreateLogEntry(auditLog, user, AuditLogActionType.Kick, ctx.User);

            await _logService.SendLogMessage("Member banned",
                $"User ❯ {ctx.Member.Mention} ({ctx.Member.DisplayName})\nResponsible User ❯ {ctx.Member.Mention}\nReason ❯ {auditLog.Reason ?? $"none, {ctx.Member.Mention} use {Formatter.InlineCode($"reason {logId} <reason>")}"}\nId ❯ {auditLog.Id}",
                ctx.Guild, auditLog.CreationTimestamp);
        }

        [Command("kick")]
        [Description("Kicks a user")]
        [RequirePermissions(Permissions.KickMembers)]
        [RequireGuild]
        public async Task KickAsync(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
        {
            _utilService.CheckHierachy(user, ctx, "kick", "kicked");

            var embed = new DiscordEmbedBuilder()
                .WithTitle("You got kicked")
                .WithDescription(
                    $"Server ❯ {ctx.Guild.Name}\nResponsible User ❯ {ctx.User.Mention}\nReason ❯ {reason ?? "none"}")
                .WithColor(DiscordColor.Purple);

            await user.SendMessageAsync(embed: embed);

            await user.RemoveAsync(reason);

            var auditLog = (await ctx.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.Kick)).First();

            var logId = _logService.CreateLogEntry(auditLog, user, AuditLogActionType.Kick, ctx.User);

            await _logService.SendLogMessage("Member kicked",
                $"User ❯ {ctx.Member.Mention} ({ctx.Member.DisplayName})\nResponsible User ❯ {ctx.Member.Mention}\nReason ❯ {auditLog.Reason ?? $"none, {ctx.Member.Mention} use {Formatter.InlineCode($"reason {logId} <reason>")}"}\nId ❯ {auditLog.Id}",
                ctx.Guild, auditLog.CreationTimestamp);
        }

        [Command("clear")]
        [Description("Clears a specific amount of messages from the current channel")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireGuild]
        public async Task ClearAsync(CommandContext ctx, int count)
        {
            var messages = (await ctx.Channel.GetMessagesAsync(count + 1)).AsEnumerable();
            messages = messages.Where(x => x.Timestamp.DateTime >= DateTime.Now - TimeSpan.FromDays(14));
            await ctx.Channel.DeleteMessagesAsync(messages);
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Channel cleared")
                .WithDescription($"Deleted the last {Formatter.InlineCode($"{messages.Count() - 1}")} messages")
                .WithColor(DiscordColor.Purple);
            var message = await ctx.RespondAsync(embed: embed);
            await _logService.SendLogMessage("Chat cleared",
                $"Responsible User ❯ {ctx.Member.Mention}\nMessages ❯ {messages.Count() - 1}\nReason ❯ none",
                ctx.Guild, DateTimeOffset.Now);
            await Task.Delay(TimeSpan.FromSeconds(15)).ContinueWith(_ => message.DeleteAsync());
        }

        [Command("bulk")]
        [Description("Deletes all messages from a channel")]
        [RequirePermissions(Permissions.ManageChannels)]
        [RequireGuild]
        [Hidden]
        public async Task BulkAsync(CommandContext ctx)
        {
            await ctx.Channel.DeleteAsync();
            var channel = await ctx.Guild.CreateTextChannelAsync(ctx.Channel.Name, ctx.Channel.Parent,
                ctx.Channel.PermissionOverwrites.AsEnumerable().Select(async x =>
                {
                    var overwrite = new DiscordOverwriteBuilder{Allowed = x.Allowed, Denied = x.Denied};
                    if (x.Type == OverwriteType.Member)
                    {
                        overwrite.For(await x.GetMemberAsync());
                    }
                    else
                    {
                        overwrite.For(await x.GetRoleAsync());
                    }

                    return overwrite;
                }).Select(x => x.Result), ctx.Channel.IsNSFW);
            var embed = new DiscordEmbedBuilder()
                .WithTitle("All messages deleted")
                .WithDescription("Deleted all messages from this channel")
                .WithColor(DiscordColor.Purple);
            var message = await channel.SendMessageAsync(embed: embed);
            await _logService.SendLogMessage("Bulk delete",
                $"Responsible User ❯ {ctx.Member.Mention}\nChannel ❯ {channel.Mention}{channel}\nReason ❯ none",
                ctx.Guild, DateTimeOffset.Now);
            await Task.Delay(TimeSpan.FromSeconds(15)).ContinueWith(_ => message.DeleteAsync());
        }
    }
}