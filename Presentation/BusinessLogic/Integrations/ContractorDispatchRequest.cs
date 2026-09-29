namespace BusinessLogic.Integrations
{
    public sealed class ContractorDispatchRequest
    {
        public int RequestId { get; init; }
        public string Category { get; init; }
        public string Location { get; init; }
        public string Description { get; init; }
        public string Priority { get; init; }
    }
}
