using Models;
using System;

namespace BusinessLogic.Notifications
{
    public class SmsNotificationStrategy : INotificationStrategy
    {
        private readonly string _smsGateway;

        public SmsNotificationStrategy(string smsGateway = "simulated-gateway")
        {
            _smsGateway = smsGateway;
        }

        public bool Send(ServiceRequest request, string message, string recipient)
        {
            // Simulate sending an SMS message
            Console.WriteLine($"[SMS] To: {recipient} | Req:{request.RequestID} | Msg: {message}");
            return true;
        }
    }
}
