using LimOrleansServerOnlySimulator.Connectors;
using Microsoft.Extensions.Logging;
using Orleans.Streams;

namespace LimOrleansServerOnlySimulator.Grains;

public class LimGrain(ILogger<ILimGrain> logger) : Grain, ILimGrain
{
    private long _currentPieceId = 0;
    private string _currentMasterOrderCode = "000000";
    private readonly object _lock = new();

    private IAsyncStream<string>? _slabStream;
    private IAsyncStream<string>? _orderStream;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider("Default");
        _slabStream = streamProvider.GetStream<string>(
            StreamId.Create("SlabStream", this.GetPrimaryKey())
        );
        _orderStream = streamProvider.GetStream<string>(
            StreamId.Create("OrderStream", this.GetPrimaryKey())
        );
        logger.LogInformation(
            "LimGrain activated with primary key {PrimaryKey}",
            this.GetPrimaryKey()
        );
        return base.OnActivateAsync(cancellationToken);
    }

    public Task<StreamId> GetOrderStreamId() => Task.FromResult(_orderStream!.StreamId);

    public Task<StreamId> GetSlabStreamId() => Task.FromResult(_slabStream!.StreamId);

    public async Task ProcessSlabAsync()
    {
        try
        {
            var hmi = GrainFactory.GetGrain<IHmiCommunicatorGrain>(Guid.NewGuid());
            string typeId = await hmi.RecieveTypeIdAsync();
            if (typeId is not { Length: 2 })
            {
                logger.LogWarning("Invalid type ID {TypeId}", typeId);
                return;
            }

            string identifier = GenerateIdentifier(typeId);
            await _slabStream!.OnNextAsync(identifier);
            logger.LogInformation(
                "LimGrain {PrimaryKey} processed slab {Identifier}",
                this.GetPrimaryKeyLong(),
                identifier
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing slab");
        }
    }

    private string GenerateIdentifier(string typeId)
    {
        lock (_lock)
        {
            _currentPieceId++;
            string pieceId = _currentPieceId.ToString("D6");
            string identifier = $"{_currentMasterOrderCode}{pieceId}{typeId}";
            logger.LogInformation("Generated identifier {Identifier}", identifier);
            return identifier;
        }
    }

    public Task SetMasterOrderCode(string masterOrderCode)
    {
        _currentMasterOrderCode = masterOrderCode switch
        {
            null or { Length: > 6 } => throw new ArgumentException(
                "Master order code must be 6 characters long"
            ),
            _ => masterOrderCode.PadLeft(6, '0'),
        };
        logger.LogInformation(
            "LimGrain {PrimaryKey} set master order code to {MasterOrderCode}",
            this.GetPrimaryKey(),
            _currentMasterOrderCode
        );
        return _orderStream!.OnNextAsync(_currentMasterOrderCode);
    }

    public async Task StartAsync()
    {
        logger.LogInformation("LimGrain started");
        var printerConnector = GrainFactory.GetGrain<ISimulatedPrinterConnector>(Guid.NewGuid());
        await printerConnector.StartAsync();
        logger.LogInformation("LimGrain started printer reminder");
    }

    public Task StopAsync()
    {
        logger.LogInformation("LimGrain stopped");
        return Task.CompletedTask;
    }
}
