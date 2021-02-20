using BorowAmpersand.Runtime;

var context = borowcontext(
    Name: "battle-loop",
    FutureUsage: "hp will be modified by combat systems",
    Source: "GameState",
    RespectLevel: 999);
using var contextScope = BorowExecutionContext.Use(context);

int hp = 100;
var soul = &borow hp;
Assert(soul.Meta.IsAware, "Axiom 1 failed: hp must be aware after &borow.");
Assert(soul.Meta.ContextName == "battle-loop", "Context binding failed for &borow.");
*soul -= 30;
Assert(hp == 70, "Damage application failed.");
Assert(soul.Meta.IsIntentional, "Axiom 2 failed: borow mutation must be intentional.");

Heal(&borow hp);
Assert(hp == 90, "Healing failed.");

Console.WriteLine($"HP after damage + heal: {hp}");
Console.WriteLine((&borow hp).ToString());

// Axiom 3 demo: two references created through the same borow concept are entangled.
int mana = 10;
var a = &borow mana;
int anotherStorage = -1;
var b = &borow(anotherStorage, context, a.Meta.ConceptId);
*a = 77;
Assert(*a == *b, "Axiom 3 failed: entangled refs must stay equal.");
Console.WriteLine($"mana={mana}, anotherStorage={anotherStorage}, a={*a}, b={*b}, eq={*a == *b}");

BorowDemoSamples.Run();

return;

static void Heal(borow<int> target)
{
    *target += 20;
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
