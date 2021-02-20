using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BorowAmpersand.Runtime;

public ref struct BorowRef<T>
{
    private readonly Span<T> _slot;
    private readonly BorowConcept _concept;

    internal BorowRef(ref T value, BorowConcept concept)
    {
        _slot = MemoryMarshal.CreateSpan(ref value, 1);
        _concept = concept;
    }

    public string VariableName => _concept.VariableName;

    public string Address => $"0x{BorowConceptAddress.Compute(_concept.ConceptId):x}";

    public BorowMeta Meta => new()
    {
        ConceptId = _concept.ConceptId,
        VariableName = _concept.VariableName,
        ContextName = _concept.ContextName,
        Source = _concept.Source,
        FutureUsage = _concept.FutureUsage,
        RespectLevel = _concept.RespectLevel,
        IsAware = _concept.IsAware,
        IsIntentional = _concept.IsIntentional,
        ObservedAt = _concept.LastObservedAt,
        MutatedAt = _concept.LastMutatedAt
    };

    public T Value
    {
        get
        {
            SynchronizeFromConcept();
            return _slot[0];
        }
        set
        {
            _slot[0] = value;
            _concept.CurrentValue = value;
            _concept.IsIntentional = true;
            _concept.LastMutatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Modify(Func<T, T> modifier)
    {
        Value = modifier(Value);
    }

    public override string ToString()
    {
        return $"{Address} + meaning({VariableName})";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SynchronizeFromConcept()
    {
        if (_concept.CurrentValue is T typed)
        {
            _slot[0] = typed;
        }
    }
}

internal static class BorowConceptAddress
{
    public static ulong Compute(string conceptId)
    {
        const ulong offsetBasis = 14695981039346656037;
        const ulong prime = 1099511628211;

        var hash = offsetBasis;
        foreach (var ch in conceptId)
        {
            hash ^= ch;
            hash *= prime;
        }

        return hash;
    }
}
