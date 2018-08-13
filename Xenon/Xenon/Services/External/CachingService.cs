#region

using System.Collections.Generic;
using Xenon.Core;

#endregion

namespace Xenon.Services.External
{
    public class CachingService
    {
        public readonly Dictionary<ulong, ExecutionObject> ExecutionObjects;

        public CachingService()
        {
            ExecutionObjects = new Dictionary<ulong, ExecutionObject>();
        }
    }
}