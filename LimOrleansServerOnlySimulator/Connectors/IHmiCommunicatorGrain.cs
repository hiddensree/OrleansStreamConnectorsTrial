public interface IHmiCommunicator : IGrainWithGuidKey
{
    Task<string> GetTypeIdAsync();
}
