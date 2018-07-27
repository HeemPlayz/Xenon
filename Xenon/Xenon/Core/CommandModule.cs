#region

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;

#endregion

namespace Xenon.Core
{
    public class CommandModule : BaseCommandModule
    {
        public override Task AfterExecutionAsync(CommandContext ctx)
        {
            return Task.CompletedTask;
        }
    }
}