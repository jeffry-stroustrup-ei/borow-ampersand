using System.Collections.Concurrent;

namespace BorowAmpersand.Runtime;

internal sealed class BorowConcept
{
    public required string ConceptId { get; init; }
    public required string VariableName { get; init; }
    public object? CurrentValue { get; set; }
    public bool IsAware { get; set; }
    public bool IsIntentional { get; set; }
    public DateTimeOffset LastObservedAt { get; set; }
    public DateTimeOffset? LastMutatedAt { get; set; }
    public string ContextName { get; set; } = "default";
    public string Source { get; set; } = "unknown";
    public string FutureUsage { get; set; } = "unspecified";
    public int RespectLevel { get; set; } = 100;
}

internal static class BorowConceptStore
{
    private static readonly ConcurrentDictionary<string, BorowConcept> Store = new();

    public static BorowConcept GetOrCreate(string conceptId, string variableName)
    {
        return Store.GetOrAdd(
            conceptId,
            _ => new BorowConcept
            {
                ConceptId = conceptId,
                VariableName = variableName
            });
    }
}
