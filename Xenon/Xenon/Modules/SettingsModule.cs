#region

using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Xenon.Core;
using Xenon.Services;

#endregion

namespace Xenon.Modules
{
    [CommandCategory(CommandCategory.Settings)]
    [CheckServer]
    public class SettingsModule : CommandBase
    {
        [Group("blocking")]
        [Summary("Shows the current blocking type or sets it")]
        public class BlockingModule : CommandBase
        {
            [Command("")]
            [Summary("Shows the current blocking type")]
            public async Task BlockingAsync()
            {
                await ReplyEmbedAsync("Blocking Type", $"The blocking type is {$"{Server.BlockingType}".InlineCode()}");
            }

            [Command("whitelist")]
            [Alias("wl")]
            [Summary("Shows the current blocking type to whitelist")]
            public async Task WhitelistAsync()
            {
                Server.BlockingType = BlockingType.Whitelist;

                await ReplyEmbedAsync("Blocking Type",
                    $"Set the blocking type to {$"{BlockingType.Whitelist}".InlineCode()}");
            }

            [Command("blacklist")]
            [Alias("bl")]
            [Summary("Shows the current blocking type to blacklist")]
            public async Task BlacklistAsync()
            {
                Server.BlockingType = BlockingType.Blacklist;

                await ReplyEmbedAsync("Blocking Type",
                    $"Set the blocking type to {$"{BlockingType.Blacklist}".InlineCode()}");
            }

            [Command("none")]
            [Alias("none")]
            [Summary("Shows the current blocking type to none")]
            public async Task NoneAsync()
            {
                Server.BlockingType = BlockingType.Whitelist;

                await ReplyEmbedAsync("Blocking Type",
                    $"Set the blocking type to {$"{BlockingType.None}".InlineCode()}");
            }
        }

        [Group("blacklist")]
        [Alias("bl")]
        [CommandCategory(CommandCategory.Settings)]
        [Summary("Displays all channels in the blacklist or lets you add/remove some")]
        public class BlacklistModule : CommandBase
        {
            [Command("")]
            [Summary("Displays all channels in the blacklist")]
            public async Task BlacklistAsync()
            {
                await ReplyEmbedAsync("Blacklist",
                    Server.Blacklist.Any()
                        ? $"The blacklist contains {string.Join(", ", Server.Blacklist.Select(x => Context.Guild.GetTextChannel(x).Mention))}"
                        : "The blacklist is empty");
            }

            [Command("add")]
            [Alias("a")]
            [Summary("Adds a channel to the blacklist")]
            [CheckPermission(GuildPermission.ManageChannels)]
            public async Task AddBlacklistAsync(params ITextChannel[] channels)
            {
                var channelCount = channels.Sum(channel => (!Server.Blacklist.Add(channel.Id) ? 0 : 1));

                await ReplyEmbedAsync("Blacklist",
                    $"Added {channelCount} {(channelCount == 1 ? "channel" : "channels")} to the blacklist");
            }

            [Command("remove")]
            [Alias("r")]
            [Summary("Removes a channel from the blacklist")]
            [CheckPermission(GuildPermission.ManageChannels)]
            public async Task RemoveBlacklistAsync(params ITextChannel[] channels)
            {
                var channelIds = channels.Select(x => x.Id);
                var channelCount = Server.Blacklist.RemoveWhere(x => channelIds.Contains(x));

                await ReplyEmbedAsync("Blacklist",
                    $"Removed {channelCount} {(channelCount == 1 ? "channel" : "channels")} from the blacklist");
            }
        }

        [Group("whitelist")]
        [Alias("wl")]
        [CommandCategory(CommandCategory.Settings)]
        [Summary("Displays all channels in the whitelist or lets you add/remove some")]
        public class WhitelistModule : CommandBase
        {
            [Command("")]
            public async Task WhitelistAsync()
            {
                await ReplyEmbedAsync("Whitelist",
                    Server.Blacklist.Any()
                        ? $"The white contains {string.Join(", ", Server.Whitelist.Select(x => Context.Guild.GetTextChannel(x).Mention))}"
                        : "The whitelist is empty");
            }

            [Command("add")]
            [Alias("a")]
            [CheckPermission(GuildPermission.ManageChannels)]
            [Summary("Adds a channel from the whitelist")]
            public async Task AddWhitelistAsync(params ITextChannel[] channels)
            {
                var channelCount = channels.Sum(channel => (!Server.Whitelist.Add(channel.Id) ? 0 : 1));

                await ReplyEmbedAsync("Whitelist",
                    $"Added {channelCount} {(channelCount == 1 ? "channel" : "channels")} to the whitelist");
            }

            [Command("remove")]
            [Alias("r")]
            [CheckPermission(GuildPermission.ManageChannels)]
            [Summary("Removes a channel from the whitelist")]
            public async Task RemoveWhitelistAsync(params ITextChannel[] channels)
            {
                var channelIds = channels.Select(x => x.Id);
                var channelCount = Server.Whitelist.RemoveWhere(x => channelIds.Contains(x));

                await ReplyEmbedAsync("Whitelist",
                    $"Removed {channelCount} {(channelCount == 1 ? "channel" : "channels")} from the whitelist");
            }
        }
    }
}