using System;
using System.Globalization;
using Cuemon;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record CoordinatedUniversalTime : SingleValueObject<string>
    {
        public static implicit operator DateTime (CoordinatedUniversalTime value)
        {
            return DateTime.Parse(value.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }

        public static implicit operator CoordinatedUniversalTime (DateTime value)
        {
            return new CoordinatedUniversalTime(value);
        }

        public CoordinatedUniversalTime(DateTimeOffset value) : this(value.UtcDateTime)
        {
        }

        public CoordinatedUniversalTime(DateTime value) : base(Validator.CheckParameter(() =>
        {
            Validator.ThrowIfFalse(value.Kind == DateTimeKind.Utc, nameof(value), "Value must be expressed as the Coordinated Universal Time (UTC).");
            return value.ToString("O");
        }))
        {
        }

        public CoordinatedUniversalTime(string value) : this(DateTime.Parse(value, CultureInfo.InvariantCulture))
        {
        }
    }
}
