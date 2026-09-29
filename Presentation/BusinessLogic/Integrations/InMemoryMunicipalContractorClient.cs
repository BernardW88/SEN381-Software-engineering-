namespace BusinessLogic.Integrations
{
    public sealed class InMemoryMunicipalContractorClient : IMunicipalContractorClient
    {
        public ContractorDispatchResult Send(MunicipalDispatchPayload payload)
        {
            if (payload == null) throw new System.ArgumentNullException(nameof(payload));

            return new ContractorDispatchResult(
                true,
                payload.Reference,
                "Dispatch accepted by development client.");
        }
    }
}
