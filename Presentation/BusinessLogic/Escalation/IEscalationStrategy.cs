using Models;

namespace BusinessLogic.Escalation
{
    public interface IEscalationStrategy
    {
        // Determines whether the given request should be escalated now.
        bool ShouldEscalate(ServiceRequest request);

        // Perform escalation action (e.g., change status, notify teams). Returns true if action performed.
        bool Escalate(ServiceRequest request);
    }
}
