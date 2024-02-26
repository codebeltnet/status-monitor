using System;
using Cuemon;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record Endpoint : SingleValueObject<string>
    {
        public static implicit operator Endpoint (string value)
        {
            return new Endpoint(value);
        }

        public Endpoint(Uri value) : this(value.OriginalString)
        {
        }

        public Endpoint(string value) : base(Validator.CheckParameter(value, () =>
        {
            Validator.ThrowIfNullOrWhitespace(value, nameof(value), $"{nameof(Endpoint)} cannot be null, empty or consist only of white-space characters.");
            Validator.ThrowIfNotUri(value, message: $"{nameof(Endpoint)} must be an absolute URI.");
        }))
        {
        }
    }
}
