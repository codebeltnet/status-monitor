using System;
using Cuemon;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record Status : SingleValueObject<string>
    {
        public const string Accepted = "Accepted";

        public Status(OperationStatus value) : this(value.ToString())
        {
        }

        public Status(string value) : base(Validator.CheckParameter(value, () =>
        {
            if (value == null || (!Enum.TryParse<OperationStatus>(value, out _) && !value.Equals(Accepted)))
            {
                throw new ArgumentOutOfRangeException(value, $"The specified '{value}' is outside the allowable range of: '{nameof(Accepted)}', '{nameof(OperationStatus.Running)}', '{nameof(OperationStatus.Succeeded)}' and '{nameof(OperationStatus.Failed)}'.");
            }
        }))
        {
        }
    }
}
