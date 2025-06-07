namespace BuildingBlocks.Exceptions
{
    public class ConflictDomainException : Exception
    {
        public ConflictDomainException(string message) : base(message)
        {
        }

        public ConflictDomainException(string message, string details) : base(message)
        {
            Details = details;
        }

        public string? Details { get; }
    }

}
