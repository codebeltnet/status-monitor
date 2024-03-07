using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cuemon.Extensions;

namespace Codebelt.Shared
{
    public static class Awaiter
    {
        public static async Task RunAsync(Func<Task<AsyncRunProgress>> action, Action<AwaiterOptions> setup = null)
        {
            var options = setup.Configure();
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed <= options.Timeout)
            {
                var runProgress = await action().ConfigureAwait(false);
                if (runProgress == AsyncRunProgress.Succeeded) { return; }
                await Task.Delay(options.Delay, options.CancellationToken).ConfigureAwait(false);
            }
        }
    }

    public enum AsyncRunProgress
    {
        Idle,
        Running,
        Failed,
        Succeeded
    }
}
