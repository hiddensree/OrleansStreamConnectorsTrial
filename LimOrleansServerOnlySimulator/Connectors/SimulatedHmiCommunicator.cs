using Microsoft.Extensions.Logging;
using Orleans;

namespace LimOrleansServer.Connectors
{
    public class SimulatedHmiCommunicator(ILogger<IHmiCommunicator> logger)
        : Grain,
            IHmiCommunicator
    {
        private readonly Random _random = new();

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "SimulatedHmiCommunicator activated with key: {Key}",
                this.GetPrimaryKey()
            );
            return Task.CompletedTask;
        }

        public Task<string> GetTypeIdAsync()
        {
            string typeId = $"T{_random.Next(1, 10)}";
            logger.LogDebug("Provided TypeId: {TypeId}", typeId);
            return Task.FromResult(typeId);
        }
    }
}
