#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Xenon.Core;
using Xenon.Services;
using ActionType = Xenon.Services.ActionType;

#endregion

namespace Xenon.Modules
{
    [CommandCategory(CommandCategory.Moderation)]
    [CheckServer]
    public class ModerationModule : CommandBase
    {
        private readonly LogService _logService;
        private readonly ServerService _serverService;

        public ModerationModule(LogService logService, ServerService serverService)
        {
            _logService = logService;
            _serverService = serverService;
        }

        [Command("ban")]
        [Description("Bans a user")]
        [CheckPermission(GuildPermission.BanMembers)]
        [CheckBotPermission(GuildPermission.BanMembers)]
        [Summary("Bans a user for a given reason")]
        public async Task BanAsync([CheckBotHierarchy] [CheckUserHierarchy]
            IGuildUser user, [Remainder] string reason = null)
        {
            await user.SendMessageAsync(embed: NormalizeEmbed("You got banned",
                    $"Server ❯ {Context.Guild.Name}\nResponsible User ❯ {Context.User.Mention}\nReason ❯ {reason ?? "none"}")
                .Build());

            await user.BanAsync(reason: reason);

            var logItem = _serverService.AddLogItem(Server, ActionType.Ban, reason, Context.User.Id, user.Id);

            await ReplyEmbedAsync("Member Banned",
                $"User ❯ {user.Mention}\nReason ❯ {reason ?? $"none, {Context.User.Mention} use {$"reason {logItem.LogId} <reason>".InlineCode()}"}");

            await _logService.SendLog(Context.Guild, "Member banned",
                $"User ❯ {Context.User.Mention} ({(Context.User as IGuildUser)?.Nickname ?? Context.User.Username})\nResponsible User ❯ {Context.User.Mention}\nReason ❯ {reason ?? $"none, {Context.User.Mention} use {$"reason {logItem.LogId} <reason>".InlineCode()}"}\nId ❯ {logItem.LogId}",
                server: Server);
        }

        [Command("kick")]
        [Description("Kicks a user")]
        [CheckPermission(GuildPermission.KickMembers)]
        [CheckPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a user for a given reason")]
        public async Task KickAsync([CheckBotHierarchy] [CheckUserHierarchy]
            IGuildUser user, [Remainder] string reason = null)
        {
            await user.SendMessageAsync(embed: NormalizeEmbed("You got kicked",
                    $"Server ❯ {Context.Guild.Name}\nResponsible User ❯ {Context.User.Mention}\nReason ❯ {reason ?? "none"}")
                .Build());

            await user.KickAsync(reason);

            var logItem = _serverService.AddLogItem(Server, ActionType.Kick, reason, Context.User.Id, user.Id);

            await ReplyEmbedAsync("Member Kicked",
                $"User ❯ {user.Mention}\nReason ❯ {reason ?? $"none, {Context.User.Mention} use {$"reason {logItem.LogId} <reason>".InlineCode()}"}");

            await _logService.SendLog(Context.Guild, "Member Kicked",
                $"User ❯ {Context.User.Mention} ({(Context.User as IGuildUser)?.Nickname ?? Context.User.Username})\nResponsible User ❯ {Context.User.Mention}\nReason ❯ {reason ?? $"none, {Context.User.Mention} use {$"reason {logItem.LogId} <reason>".InlineCode()}"}\nId ❯ {logItem.LogId}",
                server: Server);
        }

        [Command("clear")]
        [Description("Clears a specific amount of messages from the current channel")]
        [CheckPermission(GuildPermission.ManageMessages)]
        [CheckPermission(ChannelPermission.ManageMessages)]
        [Summary("Clears some messages from the current chat")]
        public async Task ClearAsync(int count, string reason = null)
        {
            var messages = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
            messages = messages.Where(x => x.Timestamp >= DateTimeOffset.Now - TimeSpan.FromDays(14));
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
            await ReplyAndDeleteAsync(embed: NormalizeEmbed("Chat Cleared",
                $"Deleted the last {$"{messages.Count() - 1}".InlineCode()} messages").Build());
            await _logService.SendLog(Context.Guild, "Chat cleared",
                $"Responsible User ❯ {Context.User.Mention}\nMessages ❯ {messages.Count() - 1}\nReason ❯ none",
                server: Server);
        }

        [Command("bulk")]
        [Description("Deletes all messages from a channel")]
        [CheckPermission(ChannelPermission.ManageChannels)]
        [CheckBotPermission(ChannelPermission.ManageChannels)]
        [Summary("Deletes all messages from the current channel")]
        public async Task BulkAsync()
        {
            var oldChannel = (ITextChannel) Context.Channel;
            await oldChannel.DeleteAsync();
            var channel = (ITextChannel) await Context.Guild.CreateTextChannelAsync(Context.Channel.Name, x =>
            {
                x.IsNsfw = oldChannel.IsNsfw;
                x.Topic = oldChannel.Topic;
                x.CategoryId = oldChannel.CategoryId;
                x.Position = oldChannel.Position;
            });
            foreach (var permissionOverwrite in oldChannel.PermissionOverwrites)
                switch (permissionOverwrite.TargetType)
                {
                    case PermissionTarget.Role:
                        var role = Context.Guild.GetRole(permissionOverwrite.TargetId);
                        await channel.AddPermissionOverwriteAsync(role, permissionOverwrite.Permissions);
                        break;
                    case PermissionTarget.User:
                        var user = Context.Guild.GetUser(permissionOverwrite.TargetId);
                        await channel.AddPermissionOverwriteAsync(user, permissionOverwrite.Permissions);
                        break;
                }
            await Interactive.ReplyAndDeleteAsync(channel,
                embed: NormalizeEmbed("Chat Cleared", "Deleted all messages from this channel").Build());
            await _logService.SendLog(Context.Guild, "Bulk delete",
                $"Responsible User ❯ {Context.User.Mention}\nChannel ❯ {channel.Mention}\nReason ❯ none",
                server: Server);
        }
    }
}