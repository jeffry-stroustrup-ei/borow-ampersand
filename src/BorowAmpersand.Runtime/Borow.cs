using System.Runtime.CompilerServices;

namespace BorowAmpersand.Runtime;

public static class Borow
{
    public static BorowRef<T> Ampersand<T>(
        ref T value,
        BorowContext? context = null,
        string? conceptId = null,
        [CallerArgumentExpression(nameof(value))] string variableExpression = "",
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
    {
        if (string.IsNullOrWhiteSpace(variableExpression))
        {
            throw BorowDiagnostics.NoPersonality(variableExpression);
        }

        var resolvedContext = context ?? BorowExecutionContext.Current;
        var resolvedConceptId = string.IsNullOrWhiteSpace(conceptId)
            ? BuildFallbackConceptId(callerFile, variableExpression)
            : conceptId;
        var concept = BorowConceptStore.GetOrCreate(resolvedConceptId, variableExpression);

        concept.IsAware = true;
        concept.LastObservedAt = DateTimeOffset.UtcNow;
        concept.ContextName = resolvedContext?.Name ?? "default";
        concept.Source = resolvedContext?.Source ?? $"{callerFile}:{callerLine}";
        concept.FutureUsage = resolvedContext?.FutureUsage ?? "undeclared";
        concept.RespectLevel = resolvedContext?.RespectLevel ?? 100;

        if (concept.CurrentValue is T existing)
        {
            value = existing;
        }
        else
        {
            concept.CurrentValue = value;
        }

        return new BorowRef<T>(ref value, concept);
    }

    private static string BuildFallbackConceptId(string callerFile, string variableExpression)
    {
        return $"{callerFile}:{variableExpression}";
    }
}
