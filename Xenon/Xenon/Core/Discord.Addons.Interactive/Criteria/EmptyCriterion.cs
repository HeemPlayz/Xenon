#region

using System.Threading.Tasks;
using Discord.Commands;

#endregion

namespace Xenon.Core.Discord.Addons.Interactive.Criteria
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, T parameter)
        {
            return Task.FromResult(true);
        }
    }
}