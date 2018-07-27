#region

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
    [Group("logchannel")]
    [Aliases("lchannel", "logc", "lc")]
    [RequireGuild]
    public class LogChannelModule : CommandModule
    {
        private readonly DatabaseService _databaseService;

        public LogChannelModule(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [GroupCommand]
        public async Task LogChannelAsync(CommandContext ctx)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder().WithTitle("Logchannel");
            var channel = ctx.Guild.GetChannel(server.LogChannelId.GetValueOrDefault());
            embed.WithColor(DiscordColor.Purple)
                .WithDescription(channel == null ? "No logchannel set" : $"The logchannel is {channel.Mention}");

            await ctx.RespondAsync(embed: embed);
        }

        [GroupCommand]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task LogChannelAsync(CommandContext ctx, DiscordChannel channel)
        {
            var channelPermissions = ctx.Guild.CurrentMember.PermissionsIn(channel);
            var embed = new DiscordEmbedBuilder();
            if (!channelPermissions.HasPermission(Permissions.AccessChannels) ||
                !channelPermissions.HasPermission(Permissions.SendMessages))
            {
                embed.WithTitle("Missing Permissions")
                    .WithColor(DiscordColor.Purple)
                    .WithDescription($"I cannot access the channel {channel.Mention}");
            }
            else
            {
                var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
                server.LogChannelId = channel.Id;
                embed
                    .WithTitle("Logchannel set")
                    .WithColor(DiscordColor.Purple)
                    .WithDescription($"Set the logchannel to {channel.Mention}");
                _databaseService.AddOrUpdateObject(server, ctx.Guild.Id);
            }

            await ctx.RespondAsync(embed: embed);
        }
    }
}