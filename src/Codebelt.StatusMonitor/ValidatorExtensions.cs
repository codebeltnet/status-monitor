using System;
using System.Runtime.CompilerServices;
using Cuemon;
using Cuemon.Extensions;

namespace Codebelt.StatusMonitor
{
    public static class ValidatorExtensions
    {
        public static void ContainsAny(this Validator _, string argument, char[] chars, [CallerArgumentExpression(nameof(argument))] string paramName = null, string message = "One or more character matches was found.")
        {
            if (argument?.ContainsAny(chars) ?? false)
            {
                throw new ArgumentOutOfRangeException(paramName, argument, message);
            }
        }
    }
}
