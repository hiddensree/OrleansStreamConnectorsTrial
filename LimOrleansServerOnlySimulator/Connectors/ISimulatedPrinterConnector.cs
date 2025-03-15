namespace LimOrleansServerOnlySimulator.Connectors
{
    public interface ISimulatedPrinterConnector : IGrainWithGuidKey
    {
        [Alias("StartAsync")]
        Task StartAsync();
    }
}
