using LimOrleansServer.Simulators;
using LimOrleansServerOnlySimulator.Simulators;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace LimOrleansServer.Tasks
{
    public class ActivateLimBridge(ILogger<ActivateLimBridge> logger, IGrainFactory grainFactory)
        : IStartupTask
    {
        private readonly Guid _simulatorKey = Guid.NewGuid();

        public async Task Execute(CancellationToken cancellationToken)
        {
            logger.LogInformation("Initializing LimSimulator...");
            var simulator = grainFactory.GetGrain<ILimSimulator>(_simulatorKey);
            await simulator.StartAsync();
            logger.LogInformation("LimSimulator initialized with key: {Key}", _simulatorKey);
        }
    }
}
