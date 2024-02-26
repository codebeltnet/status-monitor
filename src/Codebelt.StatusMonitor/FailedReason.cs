using System;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record FailedReason : SingleValueObject<string>
    {
        public static implicit operator FailedReason (string value)
        {
            return new FailedReason(value);
        }

        public FailedReason(string value) : base(value)
        {
        }

        public FailedReason(Exception value) : base(value.ToString())
        {
        }
    }
}
