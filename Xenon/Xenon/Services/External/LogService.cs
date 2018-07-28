#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

#endregion

namespace Xenon.Services.External
{
    public class LogService
    {
        private readonly DatabaseService _databaseService;

        public LogService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task ChannelCreated(ChannelCreateEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x =>
                x.Permissions.HasPermission(Permissions.ViewAuditLog) ||
                x.Permissions.HasPermission(Permissions.Administrator))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.ChannelCreate)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;

            await SendLogMessage("Channel created",
                $"Channel ❯ {e.Channel.Mention}\nResponsible User ❯ {auditLog.UserResponsible.Mention}\nReason ❯ {auditLog.Reason ?? "none"}\nId ❯ {auditLog.Id}",
                e.Guild, auditLog.CreationTimestamp);
        }

        public async Task ChannelDeleted(ChannelDeleteEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x =>
                x.Permissions.HasPermission(Permissions.ViewAuditLog) ||
                x.Permissions.HasPermission(Permissions.Administrator))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.ChannelDelete)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;

            await SendLogMessage("Channel deleted",
                $"Channel ❯ {e.Channel.Name}\nResponsible User ❯ {auditLog.UserResponsible.Mention}\nReason ❯ {auditLog.Reason ?? "none"}\nId ❯ {auditLog.Id}",
                e.Guild, auditLog.CreationTimestamp);
        }

        public async Task ChannelUpdated(ChannelUpdateEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x =>
                x.Permissions.HasPermission(Permissions.ViewAuditLog) ||
                x.Permissions.HasPermission(Permissions.Administrator))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.ChannelUpdate)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;

            var description =
                $"Channel ❯ {e.ChannelAfter.Mention}\nResponsible User ❯ {auditLog.UserResponsible.Mention}\nReason ❯ {auditLog.Reason ?? "none"}\nId ❯ {auditLog.Id}\nChanges ❯";
            var oldDescription = description;
            var properties = typeof(DiscordChannel).GetProperties().Where(x =>
                x.PropertyType != typeof(IEnumerable<DiscordChannel>) &&
                x.PropertyType != typeof(IReadOnlyList<DiscordOverwrite>) &&
                x.PropertyType != typeof(IEnumerable<DiscordMember>));
            foreach (var property in properties)
            {
                var propertyBefore = property.GetValue(e.ChannelBefore);
                var propertyAfter = property.GetValue(e.ChannelAfter);
                if (propertyBefore.ToString() != propertyAfter.ToString())
                    description += $"\n❯ {property.Name} ❯ {propertyBefore} ⇒ {propertyAfter}";
            }

            if (description == oldDescription) return;

            await SendLogMessage("Channel updated", description, e.Guild, auditLog.CreationTimestamp);
        }

        public async Task GuildBanAdded(GuildBanAddEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x =>
                x.Permissions.HasPermission(Permissions.ViewAuditLog) ||
                x.Permissions.HasPermission(Permissions.Administrator))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.Ban)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;

            var logId = CreateLogEntry(auditLog, e.Member, AuditLogActionType.Ban);

            await SendLogMessage("Member banned",
                $"User ❯ {e.Member.Mention} ({e.Member.DisplayName})\nResponsible User ❯ {auditLog.UserResponsible.Mention}\nReason ❯ {auditLog.Reason ?? $"none, {auditLog.UserResponsible.Mention} use {Formatter.InlineCode($"reason {logId} <reason>")}"}\nId ❯ {auditLog.Id}",
                e.Guild, auditLog.CreationTimestamp);
        }

        public async Task GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x =>
                x.Permissions.HasPermission(Permissions.ViewAuditLog) ||
                x.Permissions.HasPermission(Permissions.Administrator))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;
            if (auditLog.ActionType != AuditLogActionType.Kick) return;

            var logId = CreateLogEntry(auditLog, e.Member, AuditLogActionType.Kick);

            await SendLogMessage("Member kicked",
                $"User ❯ {e.Member.Mention} ({e.Member.DisplayName})\nResponsible User ❯ {auditLog.UserResponsible.Mention}\nReason ❯ {auditLog.Reason ?? $"none, {auditLog.UserResponsible.Mention} use {Formatter.InlineCode($"reason {logId} <reason>")}"}\nId ❯ {auditLog.Id}",
                e.Guild, auditLog.CreationTimestamp);
        }

        public async Task GuildBanRemoved(GuildBanRemoveEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x =>
                x.Permissions.HasPermission(Permissions.ViewAuditLog) ||
                x.Permissions.HasPermission(Permissions.Administrator))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.Unban)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;

            var logId = CreateLogEntry(auditLog, e.Member, AuditLogActionType.Ban);

            await SendLogMessage("Member unbanned",
                $"User ❯ {e.Member.Mention} ({e.Member.DisplayName})\nResponsible User ❯ {auditLog.UserResponsible.Mention}\nReason ❯ {auditLog.Reason ?? $"none, {auditLog.UserResponsible.Mention} use {Formatter.InlineCode($"reason {logId} <reason>")}"}\nId ❯ {auditLog.Id}",
                e.Guild, auditLog.CreationTimestamp);
        }

        public async Task GuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x =>
                x.Permissions.HasPermission(Permissions.ViewAuditLog) ||
                x.Permissions.HasPermission(Permissions.Administrator))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;

            var description =
                $"User ❯ {e.Member.Mention}\nResponsible User ❯ {auditLog.UserResponsible.Mention}\nReason ❯ {auditLog.Reason ?? "none"}\nId ❯ {auditLog.Id}\nChanges ❯";
            if (e.NicknameBefore != e.NicknameAfter)
                description +=
                    $"\n❯ Nickname ❯ {e.NicknameBefore ?? e.Member.Username} ⇒ {e.NicknameAfter ?? e.Member.Username}";

            var difference = e.RolesBefore.Except(e.RolesAfter);
            if (difference.Any())
                description = difference.Aggregate(description,
                    (current, role) => current + $"\nRemoved ❯ {role.Mention}");

            difference = e.RolesAfter.Except(e.RolesBefore);
            if (difference.Any())
                description =
                    difference.Aggregate(description, (current, role) => current + $"\nAdded ❯ {role.Mention}");

            await SendLogMessage("Member updated", description, e.Guild, auditLog.CreationTimestamp);
        }

        public async Task GuildRoleCreated(GuildRoleCreateEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x =>
                x.Permissions.HasPermission(Permissions.ViewAuditLog) ||
                x.Permissions.HasPermission(Permissions.Administrator))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.RoleCreate)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;

            await SendLogMessage("Role created",
                $"Role ❯ {e.Role.Mention}\nResponsible User ❯ {auditLog.UserResponsible}\nReason ❯ {auditLog.Reason ?? "none"}\nId ❯ {auditLog.Id}",
                e.Guild, auditLog.CreationTimestamp);
        }

        public async Task GuildRoleDeleted(GuildRoleDeleteEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x =>
                x.Permissions.HasPermission(Permissions.ViewAuditLog) ||
                x.Permissions.HasPermission(Permissions.Administrator))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.RoleCreate)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;

            await SendLogMessage("Role deleted",
                $"Role ❯ {e.Role.Name}\nResponsible User ❯ {auditLog.UserResponsible}\nReason ❯ {auditLog.Reason ?? "none"}\nId ❯ {auditLog.Id}",
                e.Guild, auditLog.CreationTimestamp);
        }

        public async Task GuildRoleUpdated(GuildRoleUpdateEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x => x.Permissions.HasPermission(Permissions.ViewAuditLog))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.RoleUpdate)).First();
            if (auditLog.UserResponsible.Id == e.Client.CurrentUser.Id) return;

            var description =
                $"Role ❯ {e.RoleAfter.Mention}\nResponsible User ❯ {auditLog.UserResponsible.Mention}\nReason ❯ {auditLog.Reason ?? "none"}\nId ❯ {auditLog.Id}\nChanges ❯";
            var oldDescription = description;
            var properties = typeof(DiscordRole).GetProperties()
                .Where(x => x.PropertyType != typeof(int));
            foreach (var property in properties)
            {
                var propertyBefore = property.GetValue(e.RoleBefore);
                var propertyAfter = property.GetValue(e.RoleAfter);
                if (propertyBefore.ToString() != propertyAfter.ToString())
                    description += $"\n❯ {property.Name} ❯ {propertyBefore} ⇒ {propertyAfter}";
            }

            if (description == oldDescription) return;

            await SendLogMessage("Channel updated", description, e.Guild, auditLog.CreationTimestamp);
        }

        public async Task MessageDeleted(MessageDeleteEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x => x.Permissions.HasPermission(Permissions.ViewAuditLog))) return;
            var auditLog = (await e.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.MessageDelete))
                .FirstOrDefault();
            string description;
            if (auditLog == null || auditLog.CreationTimestamp < DateTimeOffset.Now - TimeSpan.FromSeconds(5))
            {
                if (e.Message.Content == null) return;
                description =
                    $"Author ❯ {e.Message.Author.Mention}\nResponsible User ❯ {e.Message.Author.Mention}\nChannel ❯ {e.Message.Channel.Mention}\nContent ❯ {e.Message.Content}\nReason ❯ none";

                await SendLogMessage("Message deleted", description, e.Guild, DateTimeOffset.Now);
            }
            else
            {
                description =
                    $"Author ❯ {e.Message.Author.Mention}\nResponsible User ❯ {auditLog.UserResponsible.Mention}\nChannel ❯ {e.Message.Channel.Mention}\nContent ❯ {e.Message.Content ?? "none"}\nReason ❯ {auditLog.Reason ?? "none"}\nId ❯ {auditLog.Id}";

                await SendLogMessage("Message deleted", description, e.Guild, auditLog.CreationTimestamp);
            }
        }

        public async Task MessageUpdated(MessageUpdateEventArgs e)
        {
            if (!e.Guild.CurrentMember.Roles.Any(x => x.Permissions.HasPermission(Permissions.ViewAuditLog))) return;
            if (e.Author.Id == e.Client.CurrentUser.Id) return;

            if (string.IsNullOrWhiteSpace(e.MessageBefore.Content) ||
                string.IsNullOrWhiteSpace(e.Message.Content)) return;

            var description =
                $"Author ❯ {e.Author.Mention}\nChannel ❯ {e.Channel}\nBefore ❯ {e.MessageBefore.Content}\nAfter ❯ {e.Message.Content}";
            await SendLogMessage("Message updated", description, e.Guild, DateTimeOffset.Now);
        }

        public async Task SendLogMessage(string title, string description, DiscordGuild guild, DateTimeOffset timestamp)
        {
            var server = _databaseService.GetObject<Server>(guild.Id);
            if (!server.LogChannelId.HasValue) return;

            var channel = guild.GetChannel(server.LogChannelId.Value);
            if (channel == null) return;

            var embed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = description,
                Color = DiscordColor.Purple,
                Timestamp = timestamp
            };
            await channel.SendMessageAsync(embed: embed);
        }

        public ulong CreateLogEntry(DiscordAuditLogEntry auditLog, DiscordMember user, AuditLogActionType actionType,
            DiscordUser actionUser = null)
        {
            var server = _databaseService.GetObject<Server>(user.Guild.Id);

            var logId = server.ModLog.Any() ? server.ModLog.Max(x => x.Value.LogId) + 1 : 0;

            server.ModLog.Add(logId,
                new ModLogItem
                {
                    ResponsibleUserId = actionUser == null ? auditLog.UserResponsible.Id : actionUser.Id,
                    LogId = logId,
                    Reason = auditLog.Reason,
                    UserId = user.Id,
                    ActionType = actionType
                });

            _databaseService.AddOrUpdateObject(server, user.Guild.Id);

            return logId;
        }
    }
}