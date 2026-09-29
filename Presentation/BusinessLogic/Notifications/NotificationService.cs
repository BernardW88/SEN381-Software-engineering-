using Models;
using System;
using System.Collections.Generic;

namespace BusinessLogic.Notifications
{
    // Orchestrator that holds a set of notification strategies and invokes them.
    public class NotificationService
    {
        private readonly List<INotificationStrategy> _strategies = new();

        public NotificationService(IEnumerable<INotificationStrategy> strategies = null)
        {
            if (strategies != null)
                _strategies.AddRange(strategies);
        }

        public void RegisterStrategy(INotificationStrategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            _strategies.Add(strategy);
        }

        public bool NotifyAll(ServiceRequest request, string message, string recipient)
        {
            bool anySucceeded = false;
            foreach (var s in _strategies)
            {
                try
                {
                    anySucceeded |= s.Send(request, message, recipient);
                }
                catch (Exception ex)
                {
                    // Keep notification failures isolated; log and continue.
                    Console.WriteLine($"Notification strategy {s.GetType().Name} failed: {ex.Message}");
                }
            }

            return anySucceeded;
        }
    }
}
