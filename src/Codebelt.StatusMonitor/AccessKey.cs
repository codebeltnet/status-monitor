using System;
using Cuemon;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record AccessKey : ValueObject
    {
        public AccessKey(TenantId tenantId, Guid value, DateTime? timeToLive = null) : this(tenantId, value.ToString("N"), timeToLive)
        {
        }

        public AccessKey(TenantId tenantId, string secret, DateTime? timeToLive = null)
        {
            Validator.ThrowIfNullOrWhitespace(secret, nameof(secret), $"{nameof(Secret)} cannot be null, empty or consist only of white-space characters.");
            Validator.ThrowIf.ContainsAny(secret, Alphanumeric.WhiteSpace.ToCharArray(), message: $"Whitespace characters are not allowed for {nameof(secret)}.");
            Validator.ThrowIfLowerThan(secret.Length, 32, nameof(secret), $"The minimum length of {nameof(secret)} was not meet.");
            Validator.ThrowIfGreaterThan(secret.Length, 128, nameof(secret), $"The maximum length of {nameof(secret)} was exceeded.");
            TenantId = tenantId;
            Secret = secret;
            Expires = timeToLive ?? DateTime.MaxValue;
            Enabled = true;
        }

        public Guid TenantId { get; }

        public string Secret { get; set; }

        public DateTime Expires { get; set; }

        public bool Enabled { get; set; }
    }
}
