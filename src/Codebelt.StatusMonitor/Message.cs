using Cuemon;
using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record Message : SingleValueObject<string>
    {
        public static implicit operator Message (string value)
        {
            return new Message(value);
        }

        public Message(string value) : base(Validator.CheckParameter(value, () =>
        {
            Validator.ThrowIfNullOrWhitespace(value, nameof(value), $"{nameof(Message)} cannot be null, empty or consist only of white-space characters.");
        }))
        {
        }
    }
}
