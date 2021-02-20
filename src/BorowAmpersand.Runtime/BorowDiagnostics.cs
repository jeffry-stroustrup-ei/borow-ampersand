namespace BorowAmpersand.Runtime;

public static class BorowDiagnostics
{
    public static InvalidOperationException TemporaryObject() =>
        new("BOR001: cannot take &borow of temporary object. Temporary objects have no life purpose.");

    public static InvalidOperationException NoPersonality(string variableExpression) =>
        new($"BOR404: variable '{variableExpression}' has no personality.");
}
