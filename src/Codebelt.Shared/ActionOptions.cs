using System;
using Cuemon.Threading;

namespace Codebelt.Shared
{
    public class ActionOptions : AsyncOptions
    {
        public ActionOptions()
        {
            Timeout = TimeSpan.FromSeconds(5);
            Delay = TimeSpan.FromMilliseconds(100);
        }

        public TimeSpan Timeout { get; set; }
        
        public TimeSpan Delay { get; set; }
    }
}
