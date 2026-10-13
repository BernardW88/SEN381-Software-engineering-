using System;

namespace BusinessLogic.Infrastructure
{
    public sealed class ServiceRequestDomainException : Exception
    {
        public ServiceRequestDomainException(string message) : base(message)
        {
        }
    }
}
