using System;
using Cuemon;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record EndpointRouteValue : SingleValueObject<string>
    {
        public static implicit operator EndpointRouteValue (string value)
        {
            return new EndpointRouteValue(value);
        }

        public EndpointRouteValue(string value) : base(Validator.CheckParameter(value, () =>
        {
            Validator.ThrowIfNullOrWhitespace(value, nameof(value), $"{nameof(EndpointRouteValue)} cannot be null, empty or consist only of white-space characters.");
            //Validator.ThrowIfNotUri(value, uriKind: UriKind.Relative, message: $"{nameof(EndpointRouteValue)} must be a relative URI.");
        }))
        {
        }
    }
}
