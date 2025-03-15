using LimOrleansServerOnlySimulator.Grains;
using Microsoft.Extensions.Logging;
using Orleans.Streams;

namespace LimOrleansServerOnlySimulator.Simulators;

public class LimSimulator(ILogger<LimSimulator> logger) : Grain, ILimSimulator, IRemindable
{
    private readonly Random _random = new();
    private bool _isRunning = false;
    private IAsyncStream<string>? _slabStream;
    private IAsyncStream<string>? _orderStream;
    private ILimGrain? _limGrain;

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _limGrain = GrainFactory.GetGrain<ILimGrain>(this.GetPrimaryKey());
        var streamProvider = this.GetStreamProvider("Default");
        _slabStream = streamProvider.GetStream<string>(await _limGrain.GetSlabStreamId());
        _orderStream = streamProvider.GetStream<string>(await _limGrain.GetOrderStreamId());
        logger.LogInformation(
            "LimSimulator activated with primary key {PrimaryKey}",
            this.GetPrimaryKey()
        );

        await StartAsync();
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task StartAsync()
    {
        if (_isRunning)
            return;
        _isRunning = true;
        logger.LogInformation("LimSimulator {PrimaryKey} started", this.GetPrimaryKey());

        await _limGrain!.StartAsync();
        await this.RegisterOrUpdateReminder(
            "SimulateHypermateReminder",
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1)
        );
        await this.RegisterOrUpdateReminder(
            "SimulateSlabReminder",
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1)
        );
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        switch (reminderName)
        {
            case "SimulateHypermateReminder":
                await SimulateHypermateAsync();
                break;
            case "SimulateSlabReminder":
                await SimulateSlabAsync();
                break;
        }
    }

    private async Task SimulateHypermateAsync()
    {
        string newOrderCode = _random.Next(100000, 999999).ToString("D6");
        logger.LogInformation(
            "LimSimulator {PrimaryKey} generated new order code {OrderCode}",
            this.GetPrimaryKey(),
            newOrderCode
        );
        await _orderStream!.OnNextAsync(newOrderCode); // published in the stream
    }

    private async Task SimulateSlabAsync()
    {
        logger.LogInformation(
            "Simulated slab processing for LimSimulator {PrimaryKey}",
            this.GetPrimaryKey()
        );
        await _slabStream!.OnNextAsync("Simulated slab data"); // published in the stream
    }

    public override Task OnDeactivateAsync(
        DeactivationReason reason,
        CancellationToken cancellationToken
    )
    {
        _isRunning = false;
        logger.LogInformation("LimSimulator {PrimaryKey} stopped", this.GetPrimaryKey());
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public Task StopAsync()
    {
        throw new NotImplementedException();
    }
}