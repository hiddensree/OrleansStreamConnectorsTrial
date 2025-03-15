using SlabSerializer;

public interface IPrinterConnector : IGrainWithGuidKey
{
    Task SubscribeAsync(Guid limGrainKey);
    Task PrintAsync(SlabIdentifier identifier);
}
