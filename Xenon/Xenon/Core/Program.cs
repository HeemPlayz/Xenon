#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xenon.Core;

#endregion

namespace Xenon
{
    internal class Program
    {
        private static void Main()
        {
            new DiscordBot().InitializeAsync().Wait();
        }
    }
}