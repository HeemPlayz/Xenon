#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Xenon.Core;
using Xenon.Services;
using Xenon.Services.External;

#endregion

namespace Xenon.Modules
{
    [CommandCategory(CommandCategory.General)]
    public class GeneralModule : CommandModule
    {
        private readonly ConfigurationService _configurationService;
        private readonly DatabaseService _databaseService;
        private readonly LevelingService _levelingService;

        public GeneralModule(ConfigurationService configurationService, DatabaseService databaseService,
            LevelingService levelingService)
        {
            _configurationService = configurationService;
            _databaseService = databaseService;
            _levelingService = levelingService;
        }

        [Command("serverinfo")]
        [Aliases("server")]
        [RequireGuild]
        public async Task ServerInfoAsync(CommandContext ctx)
        {
            var emojis = string.Join(" ", ctx.Guild.Emojis.Select(x => x.ToString()));
            if (emojis.Length > 1024)
            {
                emojis = emojis.Substring(0, Math.Min(1024, emojis.Length));
                emojis = emojis.Substring(0, emojis.LastIndexOf(' '));
            }

            var roles = string.Join(", ", ctx.Guild.Roles.Select(x => x.Mention));
            if (roles.Length > 512)
            {
                roles = roles.Substring(0, Math.Min(512, roles.Length));
                roles = roles.Substring(0, roles.LastIndexOf(','));
            }

            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.Guild.Name, icon_url: ctx.Guild.IconUrl)
                .WithColor(DiscordColor.Purple)
                .AddField("General Information",
                    $"{Formatter.Bold("Name")} ❯ {ctx.Guild.Name}" +
                    $"\n{Formatter.Bold("Id")} ❯ {ctx.Guild.Id}\n{Formatter.Bold("Owner")} ❯ {ctx.Guild.Owner.Mention}" +
                    $"\n{Formatter.Bold("Verification")} ❯ {ctx.Guild.VerificationLevel}" +
                    $"\n{Formatter.Bold("Afk Channel")} ❯ {(ctx.Guild.AfkChannel == null ? "None" : ctx.Guild.AfkChannel.Mention)}" +
                    $"\n{Formatter.Bold("Afk Timeout")} ❯ {ctx.Guild.AfkTimeout.Minutes()} minutes" +
                    $"\n{Formatter.Bold("Highest Role")} ❯ {ctx.Guild.Roles.OrderByDescending(x => x.Position).First().Mention}" +
                    $"\n{Formatter.Bold("Created On")} ❯ {ctx.Guild.CreationTimestamp.Humanize()}",
                    true)
                .AddField($"Members - {ctx.Guild.MemberCount}",
                    $"{Formatter.Bold("<:online:456907751420067866> Online")} ❯ {ctx.Guild.Members.Count(x => x.Presence?.Status == UserStatus.Online && !x.IsBot)}" +
                    $"\n{Formatter.Bold("<:idle:456910024984363014> Idle")} ❯ {ctx.Guild.Members.Count(x => x.Presence?.Status == UserStatus.Idle && !x.IsBot)}" +
                    $"\n{Formatter.Bold("<:donotdisturb:456910051345563678> DoNotDisturb")} ❯ {ctx.Guild.Members.Count(x => x.Presence?.Status == UserStatus.DoNotDisturb && !x.IsBot)}" +
                    $"\n{Formatter.Bold("<:streaming:456910068839874560> Streaming")} ❯ {ctx.Guild.Members.Count(x => x.Presence?.Activity.ActivityType == ActivityType.Streaming && !x.IsBot)}" +
                    $"\n{Formatter.Bold("<:offline:456910084279238666> Offline")} ❯ {ctx.Guild.MemberCount - ctx.Guild.Members.Count(x => x.Presence?.Status == UserStatus.Online || x.Presence?.Status == UserStatus.Invisible || x.Presence?.Status == UserStatus.Idle || x.Presence?.Status == UserStatus.DoNotDisturb)}" +
                    $"\n{Formatter.Bold("<:bot:456910103136829441> Bots")} ❯ {ctx.Guild.Members.Count(x => x.IsBot)}",
                    true)
                .AddField($"Roles - {ctx.Guild.Roles.Count}",
                    roles, true)
                .AddField($"Emojis - {ctx.Guild.Emojis.Count}",
                    emojis, true)
                .AddField($"Channels - {ctx.Guild.Channels.Count}",
                    $"{Formatter.Bold("Categorys")} ❯ {ctx.Guild.Channels.Count(x => x.Type == ChannelType.Category)}" +
                    $"\n{Formatter.Bold("Text Channels")} ❯ {ctx.Guild.Channels.Count(x => x.Type == ChannelType.Text)}" +
                    $"\n{Formatter.Bold("Voice Channels")} ❯ {ctx.Guild.Channels.Count(x => x.Type == ChannelType.Voice)}",
                    true);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("info")]
        [Aliases("i", "statistics", "stats")]
        public async Task InfoAsync(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.Guild.CurrentMember.DisplayName, icon_url: ctx.Client.CurrentUser.AvatarUrl)
                .WithColor(DiscordColor.Purple)
                .WithDescription($"[Official Server]({_configurationService.BotDiscordInviteLink})" +
                                 $"\n[Invite](https://discordapp.com/oauth2/authorize?client_id={ctx.Client.CurrentUser.Id}&scope=bot&permissions=2146958591)" +
                                 $"\n[Listcord](https://listcord.com/bot/{ctx.Client.CurrentUser.Id})" +
                                 $"\n{Formatter.Bold("Shards")} ❯ {ctx.Client.ShardCount}" +
                                 $"\n{Formatter.Bold("Last Restart")} ❯ {Process.GetCurrentProcess().StartTime.Humanize()}" +
                                 $"\n{Formatter.Bold("Guilds")} ❯ {ctx.Client.Guilds.Count}" +
                                 $"\n{Formatter.Bold("Users")} ❯ {ctx.Client.Guilds.Values.Sum(x => x.MemberCount)}" +
                                 $"\n{Formatter.Bold("Latency")} ❯ {ctx.Client.Ping}ms" +
                                 $"\n{Formatter.Bold("Description")} ❯ {ctx.Client.CurrentApplication.Description}" +
                                 $"\n{Formatter.Bold("Owner")} ❯ {ctx.Client.CurrentApplication.Owner.Mention} ({ctx.Client.CurrentApplication.Owner.Username}#{ctx.Client.CurrentApplication.Owner.Discriminator})");

            await ctx.RespondAsync(embed: embed);
        }

        [Command("leaderboard")]
        [Aliases("lb", "leaderb", "lboard", "top")]
        [RequireGuild]
        public async Task LeaderboardAsync(CommandContext ctx)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            var leaderboard = server.Userxps.OrderByDescending(x => x.Value.Level).ThenByDescending(x => x.Value.Xp)
                .ToList();
            var embed = new DiscordEmbedBuilder()
                .WithAuthor("Leaderboard", icon_url: ctx.Guild.IconUrl)
                .WithColor(DiscordColor.Purple);

            var description = "";
            var upper = Math.Min(leaderboard.Count, 5);
            var uprank = 0;
            for (var i = 0; i < upper + uprank; i++)
            {
                var entry = leaderboard[i];

                var user = ctx.Guild.Members.FirstOrDefault(x => x.Id == entry.Key);
                if (user == null)
                {
                    uprank++;
                    continue;
                }

                description +=
                    $"{i - uprank + 1} ❯ {user.Mention}\nLevel ❯ {entry.Value.Level} | Xp ❯ {entry.Value.Xp}/{_levelingService.GetNeededXp(entry.Value.Level)}\n";
            }

            embed.WithDescription(description);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("ping")]
        public async Task PingAsync(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Pong!")
                .WithColor(DiscordColor.Purple)
                .WithDescription($"{Formatter.Bold("Gateway Latency")} ❯ {ctx.Client.Ping}ms");

            var message = await ctx.RespondAsync(embed: embed);

            embed.Description +=
                $"\n{Formatter.Bold("Message Latency")} ❯ {(DateTime.Now - message.CreationTimestamp).TotalMilliseconds}ms";

            await message.ModifyAsync(embed: new Optional<DiscordEmbed>(embed));
        }

        [Command("profile")]
        [Aliases("p")]
        [RequireGuild]
        public async Task ProfileAsync(CommandContext ctx, DiscordMember user = null)
        {
            user = user ?? ctx.Member;

            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);

            var userxp = server.Userxps.FirstOrDefault(x => x.Key == user.Id).Value ?? new Userxp();
            var userObject = _databaseService.GetObject<User>(user.Id);
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(user.DisplayName, icon_url: user.AvatarUrl)
                .WithColor(DiscordColor.Purple)
                .WithDescription($"{Formatter.Bold("Biography")} ❯ {userObject.Bio}" +
                                 $"\n{Formatter.Bold("Id")} ❯ {user.Id}" +
                                 $"\n{Formatter.Bold("Level")} ❯ {userxp.Level}" +
                                 $"\n{Formatter.Bold("Xp")} ❯ {userxp.Xp}/{_levelingService.GetNeededXp(userxp.Level)}" +
                                 $"\n{Formatter.Bold("Joined On")} ❯ {user.JoinedAt.Humanize()}" +
                                 $"\n{Formatter.Bold("Roles")} ❯ {string.Join(", ", user.Roles.Select(x => x.Mention).TakeWhile(x => x.Length <= 100))}");

            await ctx.RespondAsync(embed: embed);
        }
    }
}