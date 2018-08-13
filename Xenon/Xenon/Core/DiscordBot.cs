#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Xenon.Services;
using Xenon.Services.External;
using Xenon.Services.Nsfw;

#endregion

namespace Xenon.Core
{
    public class DiscordBot
    {
        private readonly int _shardCount = 1;
        private DiscordShardedClient _client;
        private CommandService _commands;
        private Configuration _configuration;
        private DatabaseService _database;
        private HttpClient _http;
        private InteractiveService _interactive;
        private IServiceProvider _services;

        public async Task InitializeAsync()
        {
            _configuration = ConfigurationService.LoadNewConfig();
            _database = new DatabaseService(_configuration);
            _http = new HttpClient();
            _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DiscordBot", "0.9"));
            _client = new DiscordShardedClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 2048,
                TotalShards = _shardCount
            });
            _commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                LogLevel = LogSeverity.Debug,
                DefaultRunMode = RunMode.Async
            });
            _interactive = new InteractiveService(_client);
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_configuration)
                .AddSingleton(_database)
                .AddSingleton(_interactive)
                .AddSingleton(_http)
                .AddSingleton<Random>()
                .AddSingleton<LogService>()
                .AddSingleton<CachingService>()
                .AddSingleton<ServerService>()
                .AddSingleton<NsfwService>()
                .BuildServiceProvider();
            _services.GetService<LogService>();
            _client.MessageReceived += MessageReceived;
            _client.ReactionAdded += ReactionAdded;
            _client.Log += Log;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            await _client.LoginAsync(TokenType.Bot, _configuration.BotToken);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(
                $"[{DateTimeOffset.Now:dd.MM.yyyy HH:mm:ss}] [{message.Severity}] [{message.Source}]: ");
            Console.ResetColor();
            Console.WriteLine(message.Message ?? message.Exception.Message);
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage msg)
        {
            if (msg.Author.IsBot || !(msg is SocketUserMessage message)) return;

            var argPos = 0;

            var prefixes = new List<string>(_configuration.BotPrefixes);

            Server server = null;

            ExecutionObject executionObj;

            if (message.Channel is ITextChannel channel)
            {
                var guild = channel.Guild;
                _database.Execute(x => { server = x.Load<Server>($"{guild.Id}"); });
                switch (server.BlockingType)
                {
                    case BlockingType.Whitelist when !server.Whitelist.Contains(channel.Id):
                    case BlockingType.Blacklist when server.Blacklist.Contains(channel.Id):
                        return;
                    case BlockingType.None:
                        break;
                }

                prefixes.AddRange(server.Prefixes);

                executionObj = new ExecutionObject {Server = server};
            }
            else
            {
                executionObj = new ExecutionObject();
            }

            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos) || prefixes.Any(x =>
                    message.HasStringPrefix(x, ref argPos, StringComparison.OrdinalIgnoreCase)))
            {
                var context = new ShardedCommandContext(_client, message);
                var parameters = message.Content.Substring(argPos).TrimStart('\n', ' ');
                _services.GetService<CachingService>().ExecutionObjects[message.Id] = executionObj;
                var result = await _commands.ExecuteAsync(context, parameters, _services, MultiMatchHandling.Best);
                if (!result.IsSuccess) await HandleErrorAsync(result, context, parameters);
            }
        }

        public async Task HandleErrorAsync(IResult result, ShardedCommandContext context, string parameters)
        {
            var embed = UtilService.NormalizeEmbed(null, null, ColorType.Normal, _services.GetService<Random>());
            switch (result.Error)
            {
                case CommandError.UnknownCommand:
                    break;
                case CommandError.ParseFailed:
                case CommandError.BadArgCount:
                case CommandError.ObjectNotFound:
                    var searchResult = _commands.Search(context, parameters);
                    embed.WithTitle(
                            $"{searchResult.Commands.First().Command.Name.Humanize(LetterCasing.Title)} Command Usage")
                        .WithDescription(searchResult.Commands.Select(x => x.Command).GetUsage(context));
                    await context.Channel.SendMessageAsync(embed: embed.Build());
                    break;
                case CommandError.MultipleMatches:
                    break;
                case CommandError.UnmetPrecondition:
                    embed.WithTitle("Unmet Precondition")
                        .WithDescription(result.ErrorReason);
                    await context.Channel.SendMessageAsync(embed: embed.Build());
                    break;
                case CommandError.Unsuccessful:
                case CommandError.Exception:
                case null:
                    embed.WithTitle("Internal Error")
                        .WithDescription("I just occured an internal error! :(");
                    await context.Channel.SendMessageAsync(embed: embed.Build());
                    break;
            }
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (Equals(reaction.Emote.Name, "#⃣"))
            {
                var message = await arg1.GetOrDownloadAsync();
                if (message.Author.IsBot) return;
                if (Regex.IsMatch(message.Content, PublicVariables.CodeBlockRegex,
                    RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    await message.Channel.SendMessageAsync(
                        $"{message.Author.Mention} ❯ {message.Content.ToHastebin(_http)}");
            }
        }
    }
}