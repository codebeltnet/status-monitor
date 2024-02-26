using System;
using Cuemon;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record TenantId : SingleValueObject<Guid>
    {
        public static implicit operator TenantId (Guid value)
        {
            return new TenantId(value);
        }

        public static implicit operator TenantId (string value)
        {
            return new TenantId(value);
        }

        public TenantId() : base(Guid.NewGuid())
        {
        }

        public TenantId(string value) : this(Guid.Parse(value))
        {
        }

        public TenantId(Guid value) : base(Validator.CheckParameter(value, () =>
        {
            Validator.ThrowIfTrue(value == Guid.Empty, nameof(value), $"{nameof(TenantId)} is not in a valid state.");
        }))
        {
        }
    }
}
