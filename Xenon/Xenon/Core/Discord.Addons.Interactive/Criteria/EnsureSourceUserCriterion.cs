﻿#region

using System.Threading.Tasks;
using Discord;
using Discord.Commands;

#endregion

namespace Xenon.Core.Discord.Addons.Interactive.Criteria
{
    public class EnsureSourceUserCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.User.Id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}