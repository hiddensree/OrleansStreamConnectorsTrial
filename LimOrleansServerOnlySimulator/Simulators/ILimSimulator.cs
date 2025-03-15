using System;

namespace LimOrleansServerOnlySimulator.Simulators;

public interface ILimSimulator : IGrainWithGuidKey
{
    Task StartAsync();
}
