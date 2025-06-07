namespace BuildingBlocks.Exceptions
{
    public class ForbiddenDomainException : Exception
    {
        public ForbiddenDomainException(string message) : base(message)
        {
        }

        public ForbiddenDomainException(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }
}
