using System;

namespace LimOrleansServerOnlySimulator.Connectors;

public interface IHmiCommunicatorGrain : IGrainWithGuidKey
{
    Task<string> RecieveTypeIdAsync();
}
