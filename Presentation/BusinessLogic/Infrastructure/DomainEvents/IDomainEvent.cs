namespace BusinessLogic.Infrastructure.DomainEvents
{
    public interface IDomainEvent
    {
        DateTime OccurredAtUtc { get; }
    }
}
