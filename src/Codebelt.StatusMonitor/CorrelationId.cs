using System;
using Cuemon;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record CorrelationId : SingleValueObject<string>
    {
        public static implicit operator CorrelationId (string value)
        {
            return new CorrelationId(value);
        }

        public CorrelationId(Guid value) : this(value.ToString("N"))
        {
        }

        public CorrelationId(string value) : base(Validator.CheckParameter(value, () =>
        {
            Validator.ThrowIfNullOrWhitespace(value, nameof(value),  $"{nameof(CorrelationId)} cannot be null, empty or consist only of white-space characters.");
            Validator.ThrowIf.ContainsAny(value, Alphanumeric.WhiteSpace.ToCharArray(), message: $"Whitespace characters are not allowed for a qualifying {nameof(CorrelationId)}.");
            Validator.ThrowIfLowerThan(value.Length, 32, nameof(value), $"The minimum length of a qualifying {nameof(CorrelationId)} was not meet.");
            Validator.ThrowIfGreaterThan(value.Length, 128, nameof(value), $"The maximum length of a qualifying {nameof(CorrelationId)} was exceeded.");
        }))
        {
        }
    }
}
