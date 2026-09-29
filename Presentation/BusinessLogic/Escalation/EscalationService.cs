using Models;
using System;
using System.Collections.Generic;

namespace BusinessLogic.Escalation
{
    public class EscalationService
    {
        private readonly List<IEscalationStrategy> _strategies = new();

        public EscalationService(IEnumerable<IEscalationStrategy> strategies = null)
        {
            if (strategies != null)
                _strategies.AddRange(strategies);
        }

        public void RegisterStrategy(IEscalationStrategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            _strategies.Add(strategy);
        }

        // Runs through strategies and attempts to escalate when appropriate.
        // Returns true if any escalation was performed.
        public bool RunEscalationFor(ServiceRequest request)
        {
            bool any = false;
            foreach (var s in _strategies)
            {
                try
                {
                    if (s.ShouldEscalate(request))
                    {
                        any |= s.Escalate(request);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Escalation strategy {s.GetType().Name} error: {ex.Message}");
                }
            }

            return any;
        }
    }
}
