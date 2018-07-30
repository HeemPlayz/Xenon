#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using Xenon.Services;
using Xenon.Services.External;

#endregion

namespace Xenon.Core
{
    public class DiscordBot
    {
        private readonly int _shardCount = 1;
        private DiscordClient _client;
        private CommandsNextExtension _commandsNext;
        private ConfigurationService _configurationService;
        private HttpClient _httpClient;
        private InteractivityExtension _interactivity;
        private LavalinkExtension _lavalink;
        private Random _random;
        private IServiceProvider _serviceProvider;

        public async Task InitializeAsync()
        {
            _configurationService = ConfigurationService.LoadNewConfig();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DiscordBot", "0.9"));
            _random = new Random();
            _client = new DiscordClient(new DiscordConfiguration
            {
                AutomaticGuildSync = true,
                AutoReconnect = true,
                DateTimeFormat = "mm/dd/yyyy",
                LogLevel = LogLevel.Debug,
                MessageCacheSize = 2048,
                ReconnectIndefinitely = true,
                TokenType = TokenType.Bot,
                Token = _configurationService.BotToken,
                UseInternalLogHandler = true,
                ShardCount = _shardCount
            });
            _interactivity =
                _client.UseInteractivity(new InteractivityConfiguration
                {
                    PaginationBehavior = TimeoutBehaviour.DeleteMessage
                });
            _lavalink = _client.UseLavalink();
            _serviceProvider = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_httpClient)
                .AddSingleton(_interactivity)
                .AddSingleton(_lavalink)
                .AddSingleton(_random)
                .AddSingleton(_configurationService)
                .AddSingleton<DatabaseService>()
                .AddSingleton<UtilService>()
                .AddSingleton<LogService>()
                .AddSingleton<LevelingService>()
                .AddSingleton<StatisticsUpdateServer>()
                .AddSingleton<RedditService>()
                .AddSingleton<ImageService>()
                .BuildServiceProvider();
            _commandsNext = _client.UseCommandsNext(new CommandsNextConfiguration
            {
                CaseSensitive = false,
                EnableDms = true,
                StringPrefixes = _configurationService.BotPrefixes,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = false,
                Services = _serviceProvider,
                EnableDefaultHelp = true,
                PrefixResolver = ResolvePrefix
            });

            // Handling Errors
            _commandsNext.CommandErrored += HandleErrorAsync;

            // Registering Modules
            _commandsNext.RegisterCommands(Assembly.GetEntryAssembly());

            // ModLog Events
            _client.ChannelCreated += _serviceProvider.GetService<LogService>().ChannelCreated;
            _client.ChannelDeleted += _serviceProvider.GetService<LogService>().ChannelDeleted;
            _client.ChannelUpdated += _serviceProvider.GetService<LogService>().ChannelUpdated;
            _client.GuildBanAdded += _serviceProvider.GetService<LogService>().GuildBanAdded;
            _client.GuildBanRemoved += _serviceProvider.GetService<LogService>().GuildBanRemoved;
            _client.GuildMemberRemoved += _serviceProvider.GetService<LogService>().GuildMemberRemoved;
            _client.GuildMemberUpdated += _serviceProvider.GetService<LogService>().GuildMemberUpdated;
            _client.GuildRoleCreated += _serviceProvider.GetService<LogService>().GuildRoleCreated;
            _client.GuildRoleDeleted += _serviceProvider.GetService<LogService>().GuildRoleDeleted;
            _client.GuildRoleUpdated += _serviceProvider.GetService<LogService>().GuildRoleUpdated;
            _client.MessageDeleted += _serviceProvider.GetService<LogService>().MessageDeleted;
            _client.MessageUpdated += _serviceProvider.GetService<LogService>().MessageUpdated;
            _client.GuildCreated += _serviceProvider.GetService<StatisticsUpdateServer>().GuildUpdate;
            _client.GuildDeleted += _serviceProvider.GetService<StatisticsUpdateServer>().GuildUpdate;
            _client.Ready += _serviceProvider.GetService<StatisticsUpdateServer>().Ready;

            await _client.ConnectAsync();
            await _client.InitializeAsync();
            await Task.Delay(-1);
        }

        private Task<int> ResolvePrefix(DiscordMessage msg)
        {
            var argPos = msg.GetMentionPrefixLength(_client.CurrentUser);
            var prefixes = new List<string>(_configurationService.BotPrefixes);
            if (msg.Channel is DiscordDmChannel)
            {
                prefixes.AddRange(_configurationService.BotPrefixes);
            }
            else
            {
                var server = _serviceProvider.GetService<DatabaseService>()
                    .GetObject<Server>(msg.Channel.GuildId);

                switch (server.BlockingType)
                {
                    case ChannelBlockingType.Whitelist when !server.MarkedChannels.Contains(msg.ChannelId):
                        return Task.FromResult(-1);
                    case ChannelBlockingType.Blacklist when server.MarkedChannels.Contains(msg.ChannelId):
                        return Task.FromResult(-1);
                    case ChannelBlockingType.None:
                        break;
                }

                prefixes.AddRange(server.Prefixes);

                for (var i = 0; argPos == -1 && i < prefixes.Count; i++)
                    argPos = msg.GetStringPrefixLength(prefixes[i]);
            }

            return Task.FromResult(argPos);
        }

        private async Task HandleErrorAsync(CommandErrorEventArgs args)
        {
            var ctx = args.Context;
            Console.WriteLine(args.Exception);
            DiscordEmbedBuilder embed = null;
            switch (args.Exception)
            {
                case ChecksFailedException checksFailedException:
                    switch (checksFailedException.FailedChecks.First())
                    {
                        case CooldownAttribute cooldownAttribute:
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Cooldown limit reached",
                                Color = DiscordColor.Purple,
                                Description =
                                    $"You can use this command again in {Formatter.InlineCode($"{Math.Round(cooldownAttribute.GetRemainingCooldown(ctx).TotalSeconds, 2)} seconds")}"
                            };
                            break;
                        case RequireBotPermissionsAttribute _:
                            var botPermissionAttributes =
                                checksFailedException.FailedChecks.OfType<RequireBotPermissionsAttribute>();
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Missing Permissions",
                                Color = DiscordColor.Purple,
                                Description =
                                    $"I need the {(botPermissionAttributes.Count() == 1 ? "permission" : "permissions")} {string.Join(", ", botPermissionAttributes.Select(x => Formatter.InlineCode($"{x.Permissions}")))} to do this"
                            };
                            break;
                        case RequireNsfwAttribute _:
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Missing Nsfw",
                                Color = DiscordColor.Purple,
                                Description = $"This channel has to be {Formatter.InlineCode("nsfw")} for me to do this"
                            };
                            break;
                        case RequireOwnerAttribute _:
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Missing Permissions",
                                Color = DiscordColor.Purple,
                                Description =
                                    $"You need to be the {Formatter.InlineCode("owner")} of this server to do this"
                            };
                            break;
                        case RequirePermissionsAttribute _:
                            var permissionAttributes =
                                checksFailedException.FailedChecks.OfType<RequirePermissionsAttribute>();
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Missing Permissions",
                                Color = DiscordColor.Purple,
                                Description =
                                    $"We both need the {(permissionAttributes.Count() == 1 ? "permission" : "permissions")} {string.Join(", ", permissionAttributes.Select(x => Formatter.InlineCode($"{x.Permissions}")))} to do this"
                            };
                            break;
                        case RequirePrefixesAttribute requirePrefixesAttribute:
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Missing Prefix",
                                Color = DiscordColor.Purple,
                                Description =
                                    $"You need the to use {(requirePrefixesAttribute.Prefixes.Length == 1 ? "the prefix" : "one of the prefixes")} {string.Join(", ", requirePrefixesAttribute.Prefixes.Select(x => Formatter.InlineCode($"{x}")))}"
                            };
                            break;
                        case RequireRolesAttribute requireRolesAttribute:
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Missing Role",
                                Color = DiscordColor.Purple,
                                Description =
                                    $"You need the to have the {Formatter.InlineCode(requireRolesAttribute.CheckMode.ToString().ToLower())} of the {(requireRolesAttribute.RoleNames.Count == 1 ? "role" : "roles")} {string.Join(", ", requireRolesAttribute.RoleNames.Select(x => Formatter.InlineCode($"{x}")))}"
                            };
                            break;
                        case RequireUserPermissionsAttribute _:
                            var userPermissionAttributes =
                                checksFailedException.FailedChecks.OfType<RequirePermissionsAttribute>();
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Missing Permissions",
                                Color = DiscordColor.Purple,
                                Description =
                                    $"You need the {(userPermissionAttributes.Count() == 1 ? "permission" : "permissions")} {string.Join(", ", userPermissionAttributes.Select(x => Formatter.InlineCode($"{x.Permissions}")))} to do this"
                            };
                            break;
                        case RequireGuildAttribute _:
                            embed = new DiscordEmbedBuilder
                            {
                                Title = "Wrong Channel",
                                Color = DiscordColor.Purple,
                                Description =
                                    $"This command is only aviable in {Formatter.InlineCode("server")} channels"
                            };
                            break;
                    }

                    break;
                case HierachyException hierachyException:
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Missing Permissions",
                        Color = DiscordColor.Purple,
                        Description = hierachyException.Message
                    };
                    break;
                case ArgumentException argsException:
                    break;
                case CommandNotFoundException _:
                    break;
                default:
                    Console.WriteLine(args.Exception);
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Internal Error",
                        Color = DiscordColor.Purple,
                        Description = "I just occured an internal error"
                    };
                    break;
            }

            await ctx.RespondAsync(embed: embed);
        }

        private bool HasMentionPrefix(string message, ref int argPos, string prefix)
        {
            if (!message.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
            argPos = prefix.Length;
            return true;
        }
    }
}