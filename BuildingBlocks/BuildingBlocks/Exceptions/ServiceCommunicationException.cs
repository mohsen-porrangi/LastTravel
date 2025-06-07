namespace BuildingBlocks.Exceptions
{
    public class ServiceCommunicationException : Exception
    {
        public ServiceCommunicationException(string message) : base(message)
        {
        }

        public ServiceCommunicationException(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }
}
