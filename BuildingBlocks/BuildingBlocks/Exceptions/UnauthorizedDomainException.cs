namespace BuildingBlocks.Exceptions
{
    public class UnauthorizedDomainException : Exception
    {
        public UnauthorizedDomainException(string message) : base(message)
        {
        }

        public UnauthorizedDomainException(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }
}
