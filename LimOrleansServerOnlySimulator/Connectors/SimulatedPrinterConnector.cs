using Microsoft.Extensions.Logging;
using Orleans.Streams;

namespace LimOrleansServerOnlySimulator.Connectors
{
    public class SimulatedPrinterConnector(ILogger<SimulatedPrinterConnector> logger)
        : Grain,
            ISimulatedPrinterConnector,
            IRemindable
    {
        private readonly ILogger<SimulatedPrinterConnector> _logger = logger;
        private readonly Guid _limGrainKey = Guid.NewGuid(); // Simulate a unique key for the LimGrain.

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var slabStream = streamProvider.GetStream<string>(
                StreamId.Create("SlabStream", _limGrainKey)
            );
            await slabStream.SubscribeAsync(
                async (data, token) =>
                {
                    _logger.LogInformation(
                        "SimulatedPrinterConnector {PrimaryKey} received slab data {Data}",
                        this.GetPrimaryKey(),
                        data
                    );
                    await Task.CompletedTask;
                }
            );

            _logger.LogInformation(
                "SimulatedPrinterConnector activated with primary key {PrimaryKey}",
                this.GetPrimaryKey()
            );

            // Register a reminder
            await this.RegisterOrUpdateReminder(
                "SimulatedPrinterReminder",
                dueTime: TimeSpan.FromMinutes(0),
                period: TimeSpan.FromMinutes(1)
            );

            await base.OnActivateAsync(cancellationToken);
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == "SimulatedPrinterReminder")
            {
                _logger.LogInformation(
                    "SimulatedPrinterConnector reminder triggered at {Time}",
                    DateTime.UtcNow
                );
                // Add your reminder handling logic here
                await Task.CompletedTask;
            }
        }

        public async Task StartAsync()
        {
            var reminder = this.GetReminder("SimulatedPrinterReminder");
            if (reminder is not null)
            {
                await this.RegisterOrUpdateReminder(
                    "SimulatedPrinterReminder",
                    dueTime: TimeSpan.FromMinutes(0),
                    period: TimeSpan.FromMinutes(1)
                );
            }
        }
    }
}
