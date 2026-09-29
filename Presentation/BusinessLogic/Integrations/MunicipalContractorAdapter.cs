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

            var payload = new MunicipalDispatchPayload
            {
                Reference = $"CC-{request.RequestId}",
                ServiceType = request.Category,
                SiteAddress = request.Location,
                Details = request.Description ?? string.Empty,
                Urgency = request.Priority
            };

            return _client.Send(payload);
        }
    }
}
