#region

using System;
using System.Globalization;
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
        [Summary("Lets you set the blocking type")]
        public class BlockingModule : CommandBase
        {
            [Command("")]
            [Summary("Shows the current blocking type")]
            public async Task BlockingAsync()
            {
                await ReplyEmbedAsync("Blocking Type",
                    $"The blocking type is {$"{Server.BlockingType}".ToLower().InlineCode()}");
            }

            [Command("whitelist")]
            [Alias("wl")]
            [Summary("Shows the current blocking type to whitelist")]
            public async Task WhitelistAsync()
            {
                Server.BlockingType = BlockingType.Whitelist;

                await ReplyEmbedAsync("Blocking Type",
                    $"Set the blocking type to {$"{BlockingType.Whitelist}".ToLower().InlineCode()}");
            }

            [Command("blacklist")]
            [Alias("bl")]
            [Summary("Shows the current blocking type to blacklist")]
            public async Task BlacklistAsync()
            {
                Server.BlockingType = BlockingType.Blacklist;

                await ReplyEmbedAsync("Blocking Type",
                    $"Set the blocking type to {$"{BlockingType.Blacklist}".ToLower().InlineCode()}");
            }

            [Command("none")]
            [Alias("none")]
            [Summary("Shows the current blocking type to none")]
            public async Task NoneAsync()
            {
                Server.BlockingType = BlockingType.Whitelist;

                await ReplyEmbedAsync("Blocking Type",
                    $"Set the blocking type to {$"{BlockingType.None}".ToLower().InlineCode()}");
            }
        }

        [Group("blacklist")]
        [Alias("bl")]
        [CommandCategory(CommandCategory.Settings)]
        [Summary("Lets you edit the blacklist")]
        [CheckServer]
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
        [Summary("Lets you edit the whitelist")]
        [CheckServer]
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

        [Group("category")]
        [Alias("categorys")]
        [CommandCategory(CommandCategory.Settings)]
        [CheckServer]
        [Summary("Lets you disable or enable categorys")]
        public class CategoryModule : CommandBase
        {
            [Command("")]
            [Summary("Shows all enabled/disabled categorys")]
            public async Task Categorys()
            {
                var categorys = Enum.GetValues(typeof(CommandCategory)).Cast<CommandCategory>();
                await ReplyEmbedAsync("Categorys",
                    $"Enabled: {string.Join(", ", categorys.Except(Server.DisabledCategories).Select(x => $"{x}".InlineCode()))}" +
                    $"\nDisabled: {string.Join(", ", Server.DisabledCategories.Select(x => $"{x}".InlineCode()))}");
            }

            [CheckPermission(GuildPermission.ManageGuild)]
            [Command("enable")]
            [Summary("Enables a category")]
            public async Task EnableAsync([Remainder] string category)
            {
                if (Enum.TryParse(typeof(CommandCategory), category, true, out var specificObj))
                {
                    var specificCategory = (CommandCategory) specificObj;
                    if (Server.DisabledCategories.Contains(specificCategory))
                    {
                        Server.DisabledCategories.Remove(specificCategory);
                        await ReplyEmbedAsync("Category Enabled",
                            $"Enabled the category {$"{specificCategory}".ToLower().InlineCode()}");
                    }
                    else
                    {
                        await ReplyEmbedAsync("Already Enabled",
                            $"{$"{specificCategory}".InlineCode()} is already enabled");
                    }
                }
                else
                {
                    await ReplyEmbedAsync("Unknown Category",
                        $"Aviable categorys: {string.Join(", ", Enum.GetValues(typeof(CommandCategory)).Cast<CommandCategory>().Select(x => $"{x}".ToLower().InlineCode()))}");
                }
            }

            [CheckPermission(GuildPermission.ManageGuild)]
            [Command("disable")]
            [Summary("Disables a category")]
            public async Task DisableAsync([Remainder] string category)
            {
                if (Enum.TryParse(typeof(CommandCategory), category, true, out var specificObj))
                {
                    var specificCategory = (CommandCategory) specificObj;
                    if (typeof(CommandCategory).GetMember($"{specificCategory}")[0]
                        .GetCustomAttributes(typeof(CannotDisableAttribute), true).Any())
                    {
                        await ReplyEmbedAsync("Cannot Disable",
                            $"You cannot disable the category {$"{specificCategory}".ToLower()}");
                        return;
                    }

                    if (Server.DisabledCategories.Contains(specificCategory))
                    {
                        await ReplyEmbedAsync("Already Disabled",
                            $"{$"{specificCategory}".InlineCode()} is already disabled");
                    }
                    else
                    {
                        Server.DisabledCategories.Add(specificCategory);
                        await ReplyEmbedAsync("Category Disabled",
                            $"Disabled the category {$"{specificCategory}".ToLower().InlineCode()}");
                    }
                }
                else
                {
                    await ReplyEmbedAsync("Unknown Category",
                        $"Aviable categorys: {string.Join(", ", Enum.GetValues(typeof(CommandCategory)).Cast<CommandCategory>().Select(x => $"{x}".ToLower().InlineCode()))}");
                }
            }
        }

        [Group("prefix")]
        [CommandCategory(CommandCategory.Settings)]
        [CheckServer]
        [Summary("Lets you add/remove custom prefixes")]
        public class PrefixModule : CommandBase
        {
            [Command("")]
            public async Task PrefixAsync()
            {
                await ReplyEmbedAsync($"Custom Prefixes",
                    Server.Prefixes.Any()
                        ? $"The custom prefixes are {string.Join(", ", Server.Prefixes.Select(x => x.InlineCode()))}"
                        : "There are no custom prefixes for this server");
            }

            [Command("add")]
            [Alias("a")]
            [CheckPermission(GuildPermission.ManageGuild)]
            [Summary("Adds a prefix")]
            public async Task AddPrefixAsync([Remainder] string prefix)
            {
                if (Server.Prefixes.Add(prefix))
                    await ReplyEmbedAsync("Prefix Added", $"Added the prefix {prefix.InlineCode()}");
                else
                    await ReplyEmbedAsync("Already Existing", $"The prefix {prefix.InlineCode()} already exists");
            }

            [Command("remove")]
            [Alias("r")]
            [CheckPermission(GuildPermission.ManageGuild)]
            [Summary("Removes a prefix")]
            public async Task RemovePrefixAsync([Remainder] string prefix)
            {
                if (Server.Prefixes.Remove(prefix))
                    await ReplyEmbedAsync("Prefix Removed", $"Removed the prefix {prefix.InlineCode()}");
                else
                    await ReplyEmbedAsync("Unknown Prefix", $"The prefix {prefix.InlineCode()} doesn't exist");
            }

            [Command("clear")]
            [Alias("c")]
            [CheckPermission(GuildPermission.ManageGuild)]
            [Summary("Removes all prefixes")]
            public async Task ClearPrefixesAsync()
            {
                Server.Prefixes.Clear();
                await ReplyEmbedAsync("Prefixes Cleared", $"Removed all custom prefixes");
            }
        }

        [Group("joinmessage")]
        [CommandCategory(CommandCategory.Settings)]
        [CheckServer]
        [Summary("Lets you set custom joinmessages")]
        public class JoinMessageModule : CommandBase
        {
            [Command("")]
            [Summary("Shows all joinmessages")]
            public async Task JoinMessageAsync()
            {
                if (!Server.JoinMessages.Any())
                {
                    await ReplyEmbedAsync("Joinmessages", "There are no custom messages");
                    return;
                }

                var messages = Server.JoinMessages.Select((t, i) => Server.JoinMessages.ToList()[i])
                    .Select((item, i) => $"{i + 1}. {item.InlineCode()}").ToList();

                await ReplyEmbedAsync("Joinmessages", $"The joinmessages are\n{string.Join("\n", messages)}");
            }

            [Command("add")]
            [Alias("a")]
            [Summary("Adds a joinmessage")]
            [CheckPermission(GuildPermission.ManageGuild)]
            public async Task AddJoinMessageAsync([Remainder] string message)
            {
                if (Server.JoinMessages.Add(message))
                    await ReplyEmbedAsync("Joinmessage Added", $"Added the joinmessage {message.InlineCode()}");
                else
                    await ReplyEmbedAsync("Joinmessage Exists", "This joinmessage already exists");
            }

            [Command("remove")]
            [Alias("r")]
            [Summary("Removes a joinmessage")]
            [CheckPermission(GuildPermission.ManageGuild)]
            public async Task AddJoinMessageAsync(int id)
            {
                if (!Server.JoinMessages.Any())
                {
                    await ReplyEmbedAsync("No Joinmessages", $"There are no custom joinmessages");
                    return;
                }

                if (Server.JoinMessages.Count < id || id < 1)
                {
                    if (Server.JoinMessages.Count == 1)
                        await ReplyEmbedAsync("Message Not Found",
                            $"You can only select the number {$"1".InlineCode()}");
                    else
                        await ReplyEmbedAsync("Message Not Found",
                            $"You have to select a number between {"1".InlineCode()} and {$"{Server.JoinMessages.Count}".InlineCode()}");
                }
                else
                {
                    var message = Server.JoinMessages.ToList()[id - 1];
                    Server.JoinMessages.Remove(message);
                    await ReplyEmbedAsync("Joinmessage Removed", $"Removed the joinmessage {message.InlineCode()}");
                }
            }

            [Command("clear")]
            [Alias("c")]
            [Summary("Removes all joinmessages")]
            [CheckPermission(GuildPermission.ManageGuild)]
            public async Task ClearJoinMessageAsync()
            {
                Server.JoinMessages.Clear();
                await ReplyEmbedAsync("Joinmessages Cleared", $"Removed all custom joinmessages");
            }
        }

        [Group("leavemessage")]
        [CommandCategory(CommandCategory.Settings)]
        [CheckServer]
        [Summary("Lets you set custom leavemessages")]
        public class LeaveMessageModule : CommandBase
        {
            [Command("")]
            [Summary("Shows all leavemessages")]
            public async Task LeaveMessageAsync()
            {
                if (!Server.LeaveMessages.Any())
                {
                    await ReplyEmbedAsync("Leavemessages", "There are no custom messages");
                    return;
                }

                var messages = Server.LeaveMessages.Select((t, i) => Server.LeaveMessages.ToList()[i])
                    .Select((item, i) => $"{i + 1}. {item.InlineCode()}").ToList();

                await ReplyEmbedAsync("Leavemessages", $"The leavemessages are\n{string.Join("\n", messages)}");
            }

            [Command("add")]
            [Alias("a")]
            [Summary("Adds a leavemessage")]
            [CheckPermission(GuildPermission.ManageGuild)]
            public async Task AddLeaveMessageAsync([Remainder] string message)
            {
                if (Server.LeaveMessages.Add(message))
                    await ReplyEmbedAsync("Leavemessage Added", $"Added the leavemessage {message.InlineCode()}");
                else
                    await ReplyEmbedAsync("Leavemessage Exists", "This leavemessage already exists");
            }

            [Command("remove")]
            [Alias("r")]
            [Summary("Removes a leavemessage")]
            [CheckPermission(GuildPermission.ManageGuild)]
            public async Task AddLeaveMessageAsync(int id)
            {
                if (!Server.LeaveMessages.Any())
                {
                    await ReplyEmbedAsync("No Leavemessages", $"There are no custom leavemessages");
                    return;
                }

                if (Server.LeaveMessages.Count < id || id < 1)
                {
                    if (Server.LeaveMessages.Count == 1)
                        await ReplyEmbedAsync("Message Not Found",
                            $"You can only select the number {$"1".InlineCode()}");
                    else
                        await ReplyEmbedAsync("Message Not Found",
                            $"You have to select a number between {"1".InlineCode()} and {$"{Server.LeaveMessages.Count}".InlineCode()}");
                }
                else
                {
                    var message = Server.LeaveMessages.ToList()[id - 1];
                    Server.LeaveMessages.Remove(message);
                    await ReplyEmbedAsync("Leavemessage Removed", $"Removed the leavemessage {message.InlineCode()}");
                }
            }

            [Command("clear")]
            [Alias("c")]
            [Summary("Removes all leavemessages")]
            [CheckPermission(GuildPermission.ManageGuild)]
            public async Task ClearLeaveMessageAsync()
            {
                Server.LeaveMessages.Clear();
                await ReplyEmbedAsync("Leavemessages Cleared", $"Removed all custom leavemessages");
            }
        }

        [Group("color")]
        [CommandCategory(CommandCategory.Settings)]
        [CheckServer]
        [Summary("Lets you set custom message color")]
        public class ColorModule : CommandBase
        {
            [Command("")]
            [Summary("Shows you the current color")]
            public async Task ColorAsync()
            {
                await ReplyEmbedAsync("Color",
                    $"The custom color is {Server.DefaultColor}");
            }

            [Command("")]
            [Summary("Lets you set a custom color")]
            [CheckPermission(GuildPermission.ManageGuild)]
            public async Task SetColorAsync([Remainder] string color)
            {
                if (PublicVariables.Colors.TryGetValue(color, out var hex))
                {
                    var specificColor = new Color(uint.Parse(hex, NumberStyles.HexNumber));
                    Server.DefaultColor = specificColor;
                    await ReplyEmbedAsync("Color Set", $"Set the custom color to {$"{color}".ToLower().InlineCode()}");
                }
                else
                {
                    await ReplyEmbedAsync("Color Not Found",
                        $"Aviable colors: {string.Join(", ", PublicVariables.Colors.Select(x => $"{x}".InlineCode()))}");
                }
            }

            [Command("")]
            [Summary("Lets you set a custom color")]
            [CheckPermission(GuildPermission.ManageGuild)]
            public async Task SetColorAsync(int r, int g, int b)
            {
                if (new[] {r, g, b}.Any(x => x > 255 || x < 0))
                {
                    await ReplyEmbedAsync("Wrong Format",
                        $"Every value needs to be between {"0".InlineCode()} and {"255".InlineCode()}");
                    return;
                }

                Server.DefaultColor = new Color(r, g, b);

                await ReplyEmbedAsync("Color Set",
                    $"Set the custom color to RGB({$"{r}".InlineCode()}, {$"{g}".InlineCode()}, {$"{b}".InlineCode()})");
            }
        }
    }
}