using System;
using Cuemon.Threading;

namespace Codebelt.StatusMonitor
{
    public class QueryOptions<T> : AsyncOptions
    {
        public QueryOptions()
        {
            Filter = _ => true;
        }

        public Func<T, bool> Filter { get; set; }

        public int MaxInclusiveResultCount { get; set; } = 100;
    }
}
