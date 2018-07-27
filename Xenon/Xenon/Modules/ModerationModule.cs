#region

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
                    $"Server: {ctx.Guild.Name}\nResponsible User: {ctx.User.Mention}\nReason: {reason ?? "none"}")
                .WithColor(DiscordColor.Purple);

            await user.SendMessageAsync(embed: embed);

            await user.BanAsync(reason: reason);

            var auditLog = (await ctx.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.Ban)).First();

            var logId = _logService.CreateLogEntry(auditLog, user, AuditLogActionType.Kick, ctx.User);

            await _logService.SendLogMessage("Member banned",
                $"User: {ctx.Member.Mention} ({ctx.Member.DisplayName})\nResponsible User: {ctx.Member.Mention}\nReason: {auditLog.Reason ?? $"none, {ctx.Member.Mention} use {Formatter.InlineCode($"reason {logId} <reason>")}"}\nId: {auditLog.Id}",
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
                    $"Server: {ctx.Guild.Name}\nResponsible User: {ctx.User.Mention}\nReason: {reason ?? "none"}")
                .WithColor(DiscordColor.Purple);

            await user.SendMessageAsync(embed: embed);

            await user.RemoveAsync(reason);

            var auditLog = (await ctx.Guild.GetAuditLogsAsync(1, action_type: AuditLogActionType.Kick)).First();

            var logId = _logService.CreateLogEntry(auditLog, user, AuditLogActionType.Kick, ctx.User);

            await _logService.SendLogMessage("Member kicked",
                $"User: {ctx.Member.Mention} ({ctx.Member.DisplayName})\nResponsible User: {ctx.Member.Mention}\nReason: {auditLog.Reason ?? $"none, {ctx.Member.Mention} use {Formatter.InlineCode($"reason {logId} <reason>")}"}\nId: {auditLog.Id}",
                ctx.Guild, auditLog.CreationTimestamp);
        }
    }
}