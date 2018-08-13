#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using MoreLinq;
using Xenon.Core;
using Xenon.Services;
using Xenon.Services.External;

#endregion

namespace Xenon.Modules
{
    [CommandCategory(CommandCategory.General)]
    [Summary("Displays a message with all commands or some information about a command")]
    public class GeneralModule : CommandBase
    {
        private readonly CommandService _commands;

        public GeneralModule(CommandService commands)
        {
            _commands = commands;
        }

        [CommandCategory(CommandCategory.General)]
        [Group("help")]
        public class HelpModule : CommandBase
        {
            private readonly CommandService _commands;

            public HelpModule(CommandService commands)
            {
                _commands = commands;
            }

            [Command("")]
            public async Task HelpAsync(string command)
            {
                var result = _commands.Search(command);
                var embed = new EmbedBuilder();
                if (!result.IsSuccess)
                {
                    if (Enum.TryParse(typeof(CommandCategory), command, true, out var categoryObj))
                    {
                        var category = (CommandCategory) categoryObj;
                        var modules = _commands.Modules.Where(x =>
                            x.Attributes.OfType<CommandCategoryAttribute>().FirstOrDefault()?.Category == category);
                        var embeds = new List<EmbedBuilder>();
                        if (modules.Count() <= 8)
                        {
                            embed.WithTitle($"{category.Humanize(LetterCasing.Title)} Commands")
                                .WithColor(Color.Purple);
                            foreach (var module in modules)
                            {
                                if (string.IsNullOrWhiteSpace(module.Group))
                                {
                                    foreach (var commandInfo in module.Commands)
                                    {
                                        embed.AddField($"{commandInfo.GetName().Humanize(LetterCasing.Title)}",
                                            $"{commandInfo.Summary}\n{commandInfo.GetUsage(Context).InlineCode()}", true);
                                    }
                                }
                                else
                                {
                                    embed.AddField($"{module.Group.Humanize(LetterCasing.Title)}",
                                        $"{module.Summary}\n{module.GetUsage(Context).InlineCode()}", true);
                                }
                            }

                            await ReplyEmbedAsync(embed);
                        }
                        else
                        {
                            foreach (var mdls in modules.Batch(8))
                            {
                                var embd = NormalizeEmbed($"{category.Humanize(LetterCasing.Title)} Commands", null);
                                foreach (var module in mdls)
                                {
                                    if (string.IsNullOrWhiteSpace(module.Group))
                                    {
                                        foreach (var commandInfo in module.Commands)
                                        {
                                            embed.AddField($"{commandInfo.GetName().Humanize(LetterCasing.Title)}",
                                                $"{commandInfo.Summary}\n{commandInfo.GetUsage(Context).InlineCode()}", true);
                                        }
                                    }
                                    else
                                    {
                                        embed.AddField($"{module.Group.Humanize(LetterCasing.Title)}",
                                            $"{module.Summary}\n{module.GetUsage(Context).InlineCode()}", true);
                                    }
                                }

                            embeds.Add(embd);
                            }

                            await PagedReplyAsync(embeds);
                        }

                        return;
                    }

                    await ReplyEmbedAsync("Unknown command",
                        $"Couldn't find a command for {command}");
                    return;
                }

                var specificCommand = result.Commands.First().Command;

                embed.WithTitle($"{command.Humanize(LetterCasing.Title)} Command Help")
                    .WithDescription(result.Commands.First().Command.Summary)
                    .AddField("Usage", result.Commands.Select(x => x.Command).GetUsage(Context).InlineCode());
                if (specificCommand.Aliases.Count > 1)
                    embed.AddField("Aliases",
                        string.Join(", ", specificCommand.Aliases.Select(Formatter.InlineCode)));

                await ReplyEmbedAsync(embed);
            }

            [Command("")]
            public async Task HelpAsync()
            {
                var embeds = new List<EmbedBuilder>();
                foreach (var value in Enum.GetValues(typeof(CommandCategory)))
                {
                    var category = (CommandCategory) value;
                    var embed = NormalizeEmbed($"{category.Humanize(LetterCasing.Title)} Commands",
                        $"Use {"help <command/category>".InlineCode()} to see more information about a specific command/categoory\n\n{"< >"} indicates a required parameter\n{"( )"} indicates an optional parameter");
                    var modules = _commands.Modules.Where(x =>
                        x.Attributes.OfType<CommandCategoryAttribute>().FirstOrDefault()?.Category == category);
                    if (modules.Count() == 1)
                    {
                        foreach (var module in modules)
                        {
                            if (string.IsNullOrWhiteSpace(module.Group))
                            {
                                foreach (var commandInfo in module.Commands)
                                {
                                    embed.AddField($"{commandInfo.GetName().Humanize(LetterCasing.Title)}",
                                        $"{commandInfo.Summary}\n{commandInfo.GetUsage(Context).InlineCode()}", true);
                                }
                            }
                            else
                            {
                                embed.AddField($"{module.Group.Humanize(LetterCasing.Title)}",
                                    $"{module.Summary}\n{module.GetUsage(Context).InlineCode()}", true);
                            }
                        }
                        embeds.Add(embed);
                    }
                    else
                    {
                        var pageEmbeds = new List<EmbedBuilder>();
                        foreach (var batchedModules in modules.Batch(8))
                        {
                            var pageEmbed = NormalizeEmbed(embed);
                            foreach (var module in batchedModules)
                            {
                                if (string.IsNullOrWhiteSpace(module.Group))
                                {
                                    foreach (var commandInfo in module.Commands)
                                    {
                                        embed.AddField($"{commandInfo.GetName().Humanize(LetterCasing.Title)}",
                                            $"{commandInfo.Summary}\n{commandInfo.GetUsage(Context).InlineCode()}", true);
                                    }
                                }
                                else
                                {
                                    embed.AddField($"{module.Group.Humanize(LetterCasing.Title)}",
                                        $"{module.Summary}\n{module.GetUsage(Context).InlineCode()}", true);
                                }
                            }
                            pageEmbeds.Add(pageEmbed);
                        }

                        embeds.AddRange(pageEmbeds);
                    }
                }

                if (embeds.Count == 1)
                    await ReplyAsync(embed: embeds.First().Build());
                else
                    await PagedReplyAsync(embeds);
            }
        }

        [Command("ping")]
        [Summary("Displays the bot's latency")]
        public async Task PingAsync()
        {
            var watch = new Stopwatch();
            watch.Start();
            var embed = NormalizeEmbed("Ping", $"Gateway Latency ❯ {Context.Client.Latency}ms");
            var message = await ReplyAsync(embed: embed.Build());
            watch.Stop();
            embed.Description += $"\nMessage Latency ❯ {watch.Elapsed.TotalMilliseconds}ms";
            await message.ModifyAsync(x => x.Embed = embed.Build());
        }

        [Command("info")]
        [Alias("i")]
        public async Task InfoAsync()
        {
            await ReplyEmbedAsync($"{Context.Guild.CurrentUser.Nickname ?? Context.Guild.CurrentUser.Username} Information", $"[Official Server]({Configuration.BotDiscordInviteLink})" +
                                                                                                                             $"\n[Invite](https://discordapp.com/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot&permissions=2146958591)" +
                                                                                                                             $"\n[Listcord](https://listcord.com/bot/{Context.Client.CurrentUser.Id})" +
                                                                                                                             $"\nShards ❯ {Context.Client.Shards.Count}" +
                                                                                                                             $"\nLast Restart ❯ {Process.GetCurrentProcess().StartTime.Humanize()}" +
                                                                                                                             $"\nGuilds ❯ {Context.Client.Guilds.Count}" +
                                                                                                                             $"\nUsers ❯ {Context.Client.Guilds.Sum(x => x.MemberCount)}" +
                                                                                                                             $"\nLatency ❯ {Context.Client.Latency}ms");
        }

        [Command("serverinfo")]
        [Alias("server")]
        public async Task ServerInfoAsync()
        {
            var emojis = string.Join(" ", Context.Guild.Emotes.Select(x => x.ToString()));
            if (emojis.Length > 1024)
            {
                emojis = emojis.Substring(0, Math.Min(1024, emojis.Length));
                emojis = emojis.Substring(0, emojis.LastIndexOf(' '));
            }

            var roles = string.Join(", ", Context.Guild.Roles.Select(x => x.Mention));
            if (roles.Length > 512)
            {
                roles = roles.Substring(0, Math.Min(512, roles.Length));
                roles = roles.Substring(0, roles.LastIndexOf(','));
            }

            var embed = new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .AddField("General Information",
                    $"Name ❯ {Context.Guild.Name}" +
                    $"\nId ❯ {Context.Guild.Id}" +
                    $"\nOwner ❯ {Context.Guild.Owner.Mention}" +
                    $"\nVerification ❯ {Context.Guild.VerificationLevel}" +
                    $"\nAfk Channel ❯ {(Context.Guild.AFKChannel == null ? "None" : Context.Guild.AFKChannel.Name)}" +
                    $"\nAfk Timeout ❯ {Context.Guild.AFKTimeout.Minutes().TotalMinutes} minutes" +
                    $"\nHighest Role ❯ {Context.Guild.Roles.OrderByDescending(x => x.Position).First().Mention}" +
                    $"\nCreated On ❯ {Context.Guild.CreatedAt:G}",
                    true)
                .AddField($"Members - {Context.Guild.MemberCount}",
                    $"<:online:456907751420067866> Online ❯ {Context.Guild.Users.Count(x => x.Status == UserStatus.Online && !x.IsBot)}" +
                    $"\n<:idle:456910024984363014> Idle ❯ {Context.Guild.Users.Count(x => x.Status == UserStatus.Idle && !x.IsBot)}" +
                    $"\n<:donotdisturb:456910051345563678> DoNotDisturb ❯ {Context.Guild.Users.Count(x => x.Status == UserStatus.DoNotDisturb && !x.IsBot)}" +
                    $"\n<:streaming:456910068839874560> Streaming ❯ {Context.Guild.Users.Count(x => x.Activity?.Type == ActivityType.Streaming && !x.IsBot)}" +
                    $"\n<:offline:456910084279238666> Offline ❯ {Context.Guild.MemberCount - Context.Guild.Users.Count(x => x.Status == UserStatus.Offline)}" +
                    $"\n<:bot:456910103136829441> Bots ❯ {Context.Guild.Users.Count(x => x.IsBot)}",
                    true)
                .AddField($"Roles - {Context.Guild.Roles.Count}",
                    roles, true)
                .AddField($"Emojis - {Context.Guild.Emotes.Count}",
                    emojis, true)
                .AddField($"Channels - {Context.Guild.Channels.Count}",
                    $"Categorys ❯ {Context.Guild.CategoryChannels.Count}" +
                    $"\nText Channels ❯ {Context.Guild.TextChannels.Count}" +
                    $"\nVoice Channels ❯ {Context.Guild.VoiceChannels.Count}",
                    true);

            await ReplyEmbedAsync(embed);
            }
        }
    }
