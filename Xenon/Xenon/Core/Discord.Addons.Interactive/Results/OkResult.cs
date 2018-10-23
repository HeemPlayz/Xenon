#region

using Discord.Commands;

#endregion

namespace Xenon.Core.Discord.Addons.Interactive.Results
{
    public class OkResult : RuntimeResult
    {
        public OkResult(string reason = null) : base(null, reason)
        {
        }
    }
}