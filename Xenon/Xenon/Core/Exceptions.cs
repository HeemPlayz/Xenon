#region

using System;

#endregion

namespace Xenon.Core
{
    public class HierachyException : Exception
    {
        public HierachyException(string message)
        {
            Message = message;
        }

        public override string Message { get; }
    }
}