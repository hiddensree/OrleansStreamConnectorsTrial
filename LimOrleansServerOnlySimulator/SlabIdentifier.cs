namespace SlabSerializer
{
    [GenerateSerializer]
    public record SlabIdentifier(string MasterOrderCode, string PieceId, string TypeId)
    {
        [Id(0)]
        public string MasterOrderCode { get; init; } = MasterOrderCode;

        [Id(1)]
        public string PieceId { get; init; } = PieceId;

        [Id(2)]
        public string TypeId { get; init; } = TypeId;

        public string FullIdentifier => $"{MasterOrderCode}{PieceId}{TypeId}";
    }
}