namespace BusinessLogic.Infrastructure.DomainEvents
{
    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        void Handle(TEvent domainEvent);
    }
}
