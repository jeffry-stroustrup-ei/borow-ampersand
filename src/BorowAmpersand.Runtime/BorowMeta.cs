namespace BorowAmpersand.Runtime;

public sealed class BorowMeta
{
    public required string ConceptId { get; init; }
    public required string VariableName { get; init; }
    public required string ContextName { get; init; }
    public required string Source { get; init; }
    public required string FutureUsage { get; init; }
    public required int RespectLevel { get; init; }
    public required bool IsAware { get; init; }
    public required bool IsIntentional { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public DateTimeOffset? MutatedAt { get; init; }
}
