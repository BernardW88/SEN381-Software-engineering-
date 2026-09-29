namespace BusinessLogic.Integrations
{
    public interface IContractorDispatchAdapter
    {
        ContractorDispatchResult Dispatch(ContractorDispatchRequest request);
    }
}
