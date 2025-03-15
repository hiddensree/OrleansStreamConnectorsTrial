using LimOrleansServer.Grains;
using LimOrleansServerOnlySimulator.Simulators;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime; // Still needed for IGrainTimer

namespace LimOrleansServer.Simulators
{
    public class LimSimulator(ILogger<ILimSimulator> logger) : Grain, ILimSimulator
    {
        private readonly Random _random = new();
        private ILimGrain? _limGrain;
        private IGrainTimer? _slabTimer;
        private IGrainTimer? _orderTimer;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _limGrain = GrainFactory.GetGrain<ILimGrain>(this.GetPrimaryKey());
            logger.LogInformation("LimSimulator activated with key: {Key}", this.GetPrimaryKey());
            await StartAsync();
            await base.OnActivateAsync(cancellationToken);
        }

        public async Task StartAsync()
        {
            logger.LogInformation("LIM Simulator started.");
            await _limGrain!.StartAsync();

            _slabTimer = this.RegisterGrainTimer<object>(
                async _ => await _limGrain.ProcessSlabAsync(),
                null,
                TimeSpan.FromSeconds(_random.Next(2, 5)),
                TimeSpan.FromSeconds(_random.Next(2, 5))
            );

            _orderTimer = this.RegisterGrainTimer<object>(
                async _ =>
                    await _limGrain.UpdateMasterOrderCode(_random.Next(1, 999999).ToString("D6")),
                null,
                TimeSpan.FromSeconds(_random.Next(10, 20)),
                TimeSpan.FromSeconds(_random.Next(10, 20))
            );
        }

        public override Task OnDeactivateAsync(
            DeactivationReason reason,
            CancellationToken cancellationToken
        )
        {
            _slabTimer?.Dispose();
            _orderTimer?.Dispose();
            logger.LogInformation("LIM Simulator stopped.");
            return base.OnDeactivateAsync(reason, cancellationToken);
        }
    }
}
