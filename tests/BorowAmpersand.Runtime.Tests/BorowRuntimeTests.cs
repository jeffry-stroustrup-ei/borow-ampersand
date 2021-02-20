using BorowAmpersand.Runtime;

namespace BorowAmpersand.Runtime.Tests;

public sealed class BorowRuntimeTests
{
    [Fact]
    public void AmpersandMarksObservedVariableAsAware()
    {
        var hp = 100;

        var soul = Borow.Ampersand(ref hp);

        Assert.True(soul.Meta.IsAware);
    }

    [Fact]
    public void MutationIsIntentionalAndPersistsAcrossObservations()
    {
        var hp = 100;
        var soul = Borow.Ampersand(ref hp);

        soul.Value -= 25;
        var reopened = Borow.Ampersand(ref hp);

        Assert.True(reopened.Meta.IsIntentional);
        Assert.NotNull(reopened.Meta.MutatedAt);
    }

    [Fact]
    public void SharedConceptIdEntanglesDifferentStorageSlots()
    {
        var mana = 10;
        var backup = -1;

        var first = Borow.Ampersand(ref mana, conceptId: "demo:mana");
        var second = Borow.Ampersand(
            ref backup,
            conceptId: "demo:mana",
            variableExpression: nameof(mana));

        first.Value = 77;

        Assert.Equal(77, second.Value);
        Assert.Equal(first.Address, second.Address);
    }

    [Fact]
    public void AmbientContextIsAppliedWhenContextParameterIsOmitted()
    {
        var context = new BorowContext(
            Name: "battle-loop",
            FutureUsage: "hp changes in combat",
            Source: "GameState",
            RespectLevel: 900);

        using var _ = BorowExecutionContext.Use(context);
        var hp = 50;

        var soul = Borow.Ampersand(ref hp);

        Assert.Equal("battle-loop", soul.Meta.ContextName);
        Assert.Equal("GameState", soul.Meta.Source);
        Assert.Equal(900, soul.Meta.RespectLevel);
    }
}
