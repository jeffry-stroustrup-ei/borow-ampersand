using BorowAmpersand.Runtime;

public static class BorowDemoSamples
{
    public static void Run()
    {
        var healthContext = borowcontext(
            Name: "health-management",
            FutureUsage: "health will be modified by damage and healing systems",
            Source: "PlayerStats",
            RespectLevel: 500);
        using var healthScope = BorowExecutionContext.Use(healthContext);

        int health = 100;
        var healthRef = &borow health;
        Console.WriteLine($"Initial health: {healthRef.Value}");

        healthRef.Modify(h => h - 20);
        Console.WriteLine($"After damage: {healthRef.Value}");

        healthRef.Modify(h => h + 10);
        Console.WriteLine($"After healing: {healthRef.Value}");

        int score = 50;
        var scoreRef1 = &borow score;
        int backupScore = 0;
        var scoreRef2 = &borow(backupScore, healthContext, scoreRef1.Meta.ConceptId);

        scoreRef1.Value = 75;
        _ = scoreRef2.Value;
        Console.WriteLine($"score={score}, backupScore={backupScore}, ref1={scoreRef1.Value}, ref2={scoreRef2.Value}");

        ApplyBuff(&borow health, 15);
        Console.WriteLine($"Health after buff: {health}");

        var lowPriorityContext = borowcontext(
            Name: "temporary-calculation",
            FutureUsage: "temporary value for calculations",
            Source: "MathUtils",
            RespectLevel: 50);

        int tempValue = 42;
        var tempRef = &borow(tempValue, lowPriorityContext);
        Console.WriteLine(
            $"Temp value meta: {tempRef.Meta.VariableName}, context={tempRef.Meta.ContextName}, respect={tempRef.Meta.RespectLevel}");

        string playerName = "Hero";
        var nameRef = &borow playerName;
        nameRef.Modify(n => n.ToUpperInvariant());
        Console.WriteLine($"Player name: {playerName}");
    }

    private static void ApplyBuff(borow<int> stat, int amount)
    {
        stat.Modify(s => s + amount);
    }
}
