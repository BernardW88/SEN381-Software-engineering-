namespace BusinessLogic.Integrations
{
    public sealed class MunicipalContractorAdapter : IContractorDispatchAdapter
    {
        private readonly IMunicipalContractorClient _client;

        public MunicipalContractorAdapter(IMunicipalContractorClient client)
        {
            _client = client ?? throw new System.ArgumentNullException(nameof(client));
        }

        public ContractorDispatchResult Dispatch(ContractorDispatchRequest request)
        {
            if (request == null) throw new System.ArgumentNullException(nameof(request));

            MunicipalDispatchPayload payload = MapToPayload(request);
            return _client.Send(payload);
        }

        public MunicipalDispatchPayload MapToPayload(ContractorDispatchRequest request)
        {
            if (request == null) throw new System.ArgumentNullException(nameof(request));
            if (request.RequestId <= 0) throw new System.ArgumentException("RequestId must be greater than zero.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.Category)) throw new System.ArgumentException("Category is required.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.Location)) throw new System.ArgumentException("Location is required.", nameof(request));

            string normalizedPriority = string.IsNullOrWhiteSpace(request.Priority)
                ? "MEDIUM"
                : request.Priority.Trim().ToUpperInvariant();

            return new MunicipalDispatchPayload
            {
                Reference = $"CC-{request.RequestId}",
                ServiceType = request.Category.Trim(),
                SiteAddress = request.Location.Trim(),
                Details = request.Description?.Trim() ?? string.Empty,
                Urgency = normalizedPriority
            };
        }
    }
}
