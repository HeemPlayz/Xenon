#region

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using Xenon.Services.External;

#endregion

namespace Xenon.Services
{
    public class LevelingService
    {
        private readonly ConfigurationService _configurationService;
        private readonly DatabaseService _databaseService;
        private readonly Random _random;

        public LevelingService(DatabaseService databaseService, Random random,
            ConfigurationService configurationService)
        {
            _databaseService = databaseService;
            _random = random;
            _configurationService = configurationService;
        }

        public ulong GetNeededXp(ulong level)
        {
            return (level + 1) * 500;
        }

        public async Task IncreaseUserXp(MessageCreateEventArgs ctx)
        {
            if (ctx.Author.IsBot) return;
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            if (!server.Userxps.TryGetValue(ctx.Author.Id, out var userxp))
                userxp = new Userxp {UserId = ctx.Author.Id};

            userxp.Xp += (ulong) _random.Next(15, 20);

            var neededxp = GetNeededXp(userxp.Level);

            if (neededxp <= userxp.Xp)
            {
                userxp.Level++;
                if (server.LevelingState)
                {
                    var message = server.LevelUpMessages.Any()
                        ? server.LevelUpMessages.ToList()[_random.Next(server.LevelUpMessages.Count)]
                        : _configurationService.DefaultLevelUpMessage;
                    await ctx.Channel.SendMessageAsync(message.ToMessage(ctx, userxp));
                }

                userxp.Xp = 0;
            }

            server.Userxps[ctx.Author.Id] = userxp;

            _databaseService.AddOrUpdateObject(server, server.Id);
        }
    }
}