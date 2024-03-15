using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cuemon;
using Cuemon.Extensions;

namespace Codebelt.Shared
{
    public static class Awaiter
    {
        public static async Task<ConditionalValue> RunUntilSucceededOrTimeoutAsync(Func<Task<ConditionalValue>> action, Action<ActionOptions> setup = null)
        {
            var options = setup.Configure();
            var stopwatch = Stopwatch.StartNew();
            ConditionalValue conditionalValue = null;
            while (stopwatch.Elapsed <= options.Timeout)
            {
                conditionalValue = await action().ConfigureAwait(false);
                if (conditionalValue.Succeeded) { break; }
                await Task.Delay(options.Delay, options.CancellationToken).ConfigureAwait(false);
            }
            return conditionalValue ?? new UnsuccessfulValue();
        }
    }
}
