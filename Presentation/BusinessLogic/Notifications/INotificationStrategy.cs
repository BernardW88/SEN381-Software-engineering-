using Models;

namespace BusinessLogic.Notifications
{
    public interface INotificationStrategy
    {
        // Send a notification for a specific service request.
        // Return true if sent successfully (or simulated as sent).
        bool Send(ServiceRequest request, string message, string recipient);
    }

}
