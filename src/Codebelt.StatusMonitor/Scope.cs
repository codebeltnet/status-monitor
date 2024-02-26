using Savvyio.Domain;

namespace Codebelt.StatusMonitor
{
    public record Scope : SingleValueObject<string>
    {
        public static implicit operator Scope (string value)
        {
            return new Scope(value);
        }

        public Scope(OperationScope value) : this(value.ToString())
        {
        }

        public Scope(string value) : base(value)
        {
        }
    }
}
