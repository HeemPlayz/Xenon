#region

using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Xenon.Core.Discord.Addons.Interactive.Criteria;

#endregion

namespace Xenon.Core.Discord.Addons.Interactive.Paginator
{
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketReaction parameter)
        {
            var ok = parameter.UserId == sourceContext.User.Id;
            return Task.FromResult(ok);
        }
    }
}