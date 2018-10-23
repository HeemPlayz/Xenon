#region

using System.Threading.Tasks;
using Discord;
using Discord.Commands;

#endregion

namespace Xenon.Core.Discord.Addons.Interactive.Criteria
{
    public class EnsureSourceChannelCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.Channel.Id == parameter.Channel.Id;
            return Task.FromResult(ok);
        }
    }
}