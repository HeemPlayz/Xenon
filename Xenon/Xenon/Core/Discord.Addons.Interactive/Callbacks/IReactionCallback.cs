#region

using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Xenon.Core.Discord.Addons.Interactive.Criteria;

#endregion

namespace Xenon.Core.Discord.Addons.Interactive.Callbacks
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        SocketCommandContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}