namespace BusinessLogic.Integrations
{
    public sealed class ContractorDispatchResult
    {
        public ContractorDispatchResult(bool success, string externalReference, string message)
        {
            Success = success;
            ExternalReference = externalReference;
            Message = message;
        }

        public bool Success { get; }
        public string ExternalReference { get; }
        public string Message { get; }
    }
}
