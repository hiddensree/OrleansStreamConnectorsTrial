using Orleans;
using Orleans.Streams;

namespace LimOrleansServer.Grains
{
    public interface ILimGrain : IGrainWithGuidKey
    {
        Task StartAsync();
        Task ProcessSlabAsync();
        Task UpdateMasterOrderCode(string orderCode);
    }
}