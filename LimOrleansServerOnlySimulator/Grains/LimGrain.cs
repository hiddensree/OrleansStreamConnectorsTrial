using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using SlabSerializer;

namespace LimOrleansServer.Grains
{
    public class LimGrain(ILogger<ILimGrain> logger) : Grain, ILimGrain
    {
        private long _currentPieceId = 0;
        private string _currentMasterOrderCode = "000000";
        private readonly object _pieceIdLock = new();
        private IAsyncStream<SlabIdentifier>? _slabStream;
        private IAsyncStream<string>? _orderStream;

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            _slabStream = streamProvider.GetStream<SlabIdentifier>(
                StreamId.Create("SlabStream", this.GetPrimaryKey())
            );
            _orderStream = streamProvider.GetStream<string>(
                StreamId.Create("OrderStream", this.GetPrimaryKey())
            );
            logger.LogInformation("LimGrain activated with key: {Key}", this.GetPrimaryKey());
            return Task.CompletedTask;
        }

        public async Task StartAsync()
        {
            logger.LogInformation("LimGrain started.");
            var printer = GrainFactory.GetGrain<IPrinterConnector>(this.GetPrimaryKey());
            await printer.SubscribeAsync(this.GetPrimaryKey());
        }

        public async Task ProcessSlabAsync()
        {
            try
            {
                var hmi = GrainFactory.GetGrain<IHmiCommunicator>(this.GetPrimaryKey());
                string typeId = await hmi.GetTypeIdAsync();
                if (typeId is not { Length: 2 })
                {
                    logger.LogError("Invalid TypeId: {TypeId}", typeId);
                    return;
                }

                var identifier = GenerateIdentifier(typeId);
                await _slabStream!.OnNextAsync(identifier);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process slab.");
            }
        }

        public Task UpdateMasterOrderCode(string orderCode)
        {
            _currentMasterOrderCode = ValidateAndFormatOrderCode(orderCode);
            logger.LogInformation("Master Order Code updated: {Code}", _currentMasterOrderCode);
            return _orderStream!.OnNextAsync(_currentMasterOrderCode);
        }

        private SlabIdentifier GenerateIdentifier(string typeId)
        {
            lock (_pieceIdLock)
            {
                _currentPieceId++;
                string pieceId = _currentPieceId.ToString("D10");
                var identifier = new SlabIdentifier(_currentMasterOrderCode, pieceId, typeId);
                logger.LogDebug("Generated: {Identifier}", identifier.FullIdentifier);
                return identifier;
            }
        }

        private string ValidateAndFormatOrderCode(string orderCode) =>
            orderCode switch
            {
                null or { Length: > 6 } => throw new ArgumentException(
                    "Master Order Code must be 1-6 characters."
                ),
                _ => orderCode.PadLeft(6, '0'),
            };
    }
}
