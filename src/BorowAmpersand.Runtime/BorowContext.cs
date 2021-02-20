namespace BorowAmpersand.Runtime;

public sealed record BorowContext(
    string Name,
    string? FutureUsage = null,
    string? Source = null,
    int RespectLevel = 100);
