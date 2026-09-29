namespace BusinessLogic.Integrations
{
    public sealed class MunicipalDispatchPayload
    {
        public string Reference { get; init; }
        public string ServiceType { get; init; }
        public string SiteAddress { get; init; }
        public string Details { get; init; }
        public string Urgency { get; init; }
    }
}
