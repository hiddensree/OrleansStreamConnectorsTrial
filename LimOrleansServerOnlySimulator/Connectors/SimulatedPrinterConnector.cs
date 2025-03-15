using Microsoft.Extensions.Logging;
using Orleans.Streams;
using SlabSerializer;

namespace LimOrleansServer.Connectors
{
    public class SimulatedPrinterConnector(ILogger<IPrinterConnector> logger)
        : Grain,
            IPrinterConnector
    {
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "SimulatedPrinterConnector activated with key: {Key}",
                this.GetPrimaryKey()
            );
            return Task.CompletedTask;
        }

        public async Task SubscribeAsync(Guid limGrainKey)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var slabStream = streamProvider.GetStream<SlabIdentifier>(
                StreamId.Create("SlabStream", limGrainKey)
            );
            await slabStream.SubscribeAsync(
                async (identifier, token) => await PrintAsync(identifier)
            );
            logger.LogInformation("Subscribed to SlabStream for LimGrain: {Key}", limGrainKey);
        }

        public Task PrintAsync(SlabIdentifier identifier)
        {
            logger.LogInformation("Printed: {Identifier}", identifier.FullIdentifier);
            return Task.CompletedTask;
        }
    }
}
