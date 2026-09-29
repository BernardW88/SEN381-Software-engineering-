namespace BusinessLogic.Integrations
{
    public interface IMunicipalContractorClient
    {
        ContractorDispatchResult Send(MunicipalDispatchPayload payload);
    }
}
