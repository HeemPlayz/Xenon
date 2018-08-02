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
    [Group("announcechannel")]
    [Aliases("achannel", "announcec", "ac")]
    [RequireGuild]
    [CommandCategory(CommandCategory.Settings)]
    public class AnnounceChannelModule : CommandModule
    {
        private readonly DatabaseService _databaseService;

        public AnnounceChannelModule(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [GroupCommand]
        public async Task AnnounceChannelAsync(CommandContext ctx)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder().WithTitle("Announcechannel");
            var channel = ctx.Guild.GetChannel(server.AnnounceChannelId.GetValueOrDefault());
            embed.WithColor(DiscordColor.Purple)
                .WithDescription(channel == null
                    ? "No announcechannel set"
                    : $"The announcechannel is {channel.Mention}");

            await ctx.RespondAsync(embed: embed);
        }

        [GroupCommand]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task AnnounceChannelAsync(CommandContext ctx, DiscordChannel channel)
        {
            var channelPermissions = ctx.Guild.CurrentMember.PermissionsIn(channel);
            var embed = new DiscordEmbedBuilder();
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            server.AnnounceChannelId = channel.Id;
            embed
                .WithTitle("Announcechannel set")
                .WithColor(DiscordColor.Purple)
                .WithDescription($"Set the announcechannel to {channel.Mention}");
            _databaseService.AddOrUpdateObject(server, ctx.Guild.Id);


            await ctx.RespondAsync(embed: embed);
        }
    }
}