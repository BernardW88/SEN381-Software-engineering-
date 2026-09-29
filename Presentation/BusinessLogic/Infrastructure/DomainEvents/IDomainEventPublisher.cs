namespace BusinessLogic.Infrastructure.DomainEvents
{
    public interface IDomainEventPublisher
    {
        void Subscribe<TEvent>(IDomainEventHandler<TEvent> handler) where TEvent : IDomainEvent;
        void Publish<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
    }
}
