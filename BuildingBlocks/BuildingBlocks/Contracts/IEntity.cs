namespace BuildingBlocks.Contracts
{
    public interface IEntity<out T>
    {
        T Id { get; }
    }
}
