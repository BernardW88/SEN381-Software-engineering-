using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessLogic.Infrastructure.DomainEvents
{
    public sealed class InMemoryDomainEventPublisher : IDomainEventPublisher
    {
        private readonly object _sync = new();
        private readonly Dictionary<Type, List<object>> _handlers = new();

        public void Subscribe<TEvent>(IDomainEventHandler<TEvent> handler) where TEvent : IDomainEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_sync)
            {
                if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
                {
                    handlers = new List<object>();
                    _handlers[typeof(TEvent)] = handlers;
                }

                handlers.Add(handler);
            }
        }

        public void Publish<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
        {
            IDomainEventHandler<TEvent>[] handlers;

            lock (_sync)
            {
                handlers = _handlers.TryGetValue(typeof(TEvent), out var registered)
                    ? registered.Cast<IDomainEventHandler<TEvent>>().ToArray()
                    : Array.Empty<IDomainEventHandler<TEvent>>();
            }

            foreach (var handler in handlers)
            {
                try
                {
                    handler.Handle(domainEvent);
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError("Domain event handler failed: " + ex.Message);
                }
            }
        }
    }
}
