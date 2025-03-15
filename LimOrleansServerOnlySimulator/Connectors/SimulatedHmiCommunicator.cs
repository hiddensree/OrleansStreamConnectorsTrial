using System;
using Microsoft.Extensions.Logging;

namespace LimOrleansServerOnlySimulator.Connectors;

public class SimulatedHmiCommunicator(ILogger<IHmiCommunicatorGrain> logger)
    : Grain,
        IHmiCommunicatorGrain
{
    private readonly Random _random = new();

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "SimulatedHmiCommunicator activated with primary key {PrimaryKey}",
            this.GetPrimaryKey()
        );
        return base.OnActivateAsync(cancellationToken);
    }

    /// <summary>
    /// Simulates the recieving of a type ID from an HMI.
    /// </summary>
    /// <returns></returns>
    public Task<string> RecieveTypeIdAsync()
    {
        string typeId = $"Type{_random.Next(1, 10)}";
        logger.LogInformation(
            "SimulatedHmiCommunicator {PrimaryKey} recieved type ID {TypeId}",
            this.GetPrimaryKey(),
            typeId
        );
        return Task.FromResult(typeId);
    }
}
