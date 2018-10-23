#region

using System.Threading.Tasks;
using Discord.Commands;

#endregion

namespace Xenon.Core.Discord.Addons.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(SocketCommandContext sourceContext, T parameter);
    }
}