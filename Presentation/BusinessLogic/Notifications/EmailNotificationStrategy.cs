using Models;
using System;

namespace BusinessLogic.Notifications
{
    public class EmailNotificationStrategy : INotificationStrategy
    {
        private readonly string _smtpServer;

        public EmailNotificationStrategy(string smtpServer = "localhost")
        {
            _smtpServer = smtpServer;
        }

        public bool Send(ServiceRequest request, string message, string recipient)
        {
            // In a real system, integrate with SMTP / mail service.
            // Here we provide a lightweight, testable simulation.
            Console.WriteLine($"[Email] To: {recipient} | Subject: Request #{request.RequestID} - {request.Title} | Message: {message}");
            return true;
        }
    }
}
