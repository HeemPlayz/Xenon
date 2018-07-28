#region

using System;
using DSharpPlus.CommandsNext;
using Xenon.Services.External;

#endregion

namespace Xenon.Services
{
    public class LevelingService
    {
        private readonly DatabaseService _databaseService;

        public LevelingService(DatabaseService databaseService, Random random)
        {
            _databaseService = databaseService;
        }

        public ulong GetNeededXp(ulong level)
        {
            return (level + 1) * 500;
        }

        public void IncreaseUserXp(CommandContext ctx)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            Userxp userxp;
            if (!server.Userxps.TryGetValue(ctx.User.Id, out userxp)) userxp = new Userxp();
        }
    }
}