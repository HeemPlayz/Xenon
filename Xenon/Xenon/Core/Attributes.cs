#region

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

#endregion

namespace Xenon.Core
{
    public class CheckParameters : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(ctx.RawArgumentString));
        }
    }
}