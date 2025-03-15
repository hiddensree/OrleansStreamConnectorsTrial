namespace LimOrleansServerOnlySimulator.Grains;

public interface ILimGrain : IGrainWithGuidKey
{
    Task StartAsync();
    Task ProcessSlabAsync();
    Task SetMasterOrderCode(string masterOrderCode);
    Task<StreamId> GetSlabStreamId();
    Task<StreamId> GetOrderStreamId();
    Task StopAsync();
}
