using System;
using LimOrleansServerOnlySimulator.Connectors;
using LimOrleansServerOnlySimulator.Simulators;

namespace LimOrleansServerOnlySimulator;

public class ActivateLimBridge : IStartupTask
{
    private ILimSimulator _limGrain;
    private IGrainFactory _grainFactory;

    public ActivateLimBridge(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
        _limGrain = _grainFactory.GetGrain<ILimSimulator>(Guid.NewGuid());
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        Task zero = _limGrain.StartAsync();
        await zero; 
        // _grainFactory
        //     .GetGrain<ISimulatedPrinterConnector>(Guid.NewGuid())
        //     .OnActivateAsync(cancellationToken);
    }
}
