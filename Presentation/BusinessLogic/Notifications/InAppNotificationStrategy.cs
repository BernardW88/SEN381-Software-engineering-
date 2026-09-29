using Models;
using System;

namespace BusinessLogic.Notifications
{
    public class InAppNotificationStrategy : INotificationStrategy
    {
        public bool Send(ServiceRequest request, string message, string recipient)
        {
            // For in-app notifications we would persist a notification record.
            // Keep a console log for now to help during development.
            Console.WriteLine($"[InApp] User: {recipient} | Request:{request.RequestID} | {message}");
            return true;
        }
    }
}
