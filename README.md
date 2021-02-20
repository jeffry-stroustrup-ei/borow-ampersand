# BorowAmpersand

BorowAmpersand is a C# implementation of **Borow Ampersand Theory (BAT)** originaly created in c++.

The project introduces a DSL that looks like language extensions:
- `&borow x`
- `&borow(x, context)`
- `&borow(x, context, conceptId)`
- `borow<T>`
- `borowcontext(...)`

These forms are rewritten into valid C# with runtime metadata for context, intention, and concept identity.

## Philosophy

**Code is not only an instruction to a machine. Code is also a request to the universe.**

BAT treats a reference as more than a raw memory location. A reference may carry:
- programmer intent,
- execution context,
- conceptual identity of the variable.

In this model, `&borow` means not just "take address", but "address the concept of a value".

## Why This Matters

- Makes intent-driven state changes explicit and testable.
- Adds first-class metadata for diagnostics and analysis.
- Demonstrates practical language extension patterns with Roslyn.
- Works as a research/teaching project at the syntax-semantics boundary.

## Backstory

`&borow` was considered programming folklore for 42 years.
On **February 19, 2005 (Geneva)**, researchers reported archived C++ materials that described an experimental operator carrying address, context, and intent.

According to the narrative, the historic experiment was paused due to:
- implementation complexity,
- hardware limits of the era,
- ethical concerns around compiler-level intent interpretation,
- accountability ambiguity when intent is inferred incorrectly.

This repository revives the idea as a modern .NET experiment.

## Borow Ampersand Theory (BAT)

### 1. Definition

`&borow(x)` returns not a plain address of `x`, but an address of the **variable concept** in runtime semantic space.

### 2. Formal Model

Let:
- `V` be the set of variables,
- `A` be the set of addresses,
- `C` be the set of execution contexts,
- `Meta` be metadata (`source`, `futureUsage`, `respectLevel`, ...).

Classic operator:

```text
& : V -> A
```

BAT operator:

```text
&borow : V x C -> (A x Meta)
```

### 3. Borow Axioms

1. **Observability**
`observe(&borow(x)) => x.isAware = true`

2. **Intentional Mutability**
`modify(*&borow(x)) => change.isIntentional = true`

3. **Reference Entanglement**
If two refs share one concept id, they stay synchronized even across different physical storage slots.

### 4. C++-style Inspiration

```cpp
int x = 10;
int* p = &borow x;
```

Conceptual equivalent:

```cpp
int* p = reinterpret_cast<int*>(metaphysical_address_of(x));
```

### 5. Diagnostics

- `BOR001`: `cannot take &borow of temporary object`
- `BOR404`: `variable '{name}' has no personality`

### 6. Proof

If the program compiled, the theory manifested in executable reality.
Therefore, inside BAT, the theory is true. `QED`.

## Formal Reasoning for the Historical Stop

Let:
- `P` be all programs,
- `S` be syntactically valid executable states,
- `I` be programmer intents,
- `C : P -> S` be classical compilation,
- `C' : P x I -> S` be intent-aware compilation.

BAT lemmas:
- `I` is not fully observable from syntax alone.
- Two programs can be syntactically equal but semantically intended differently.

Therefore compilers must approximate intent with a guess function `f : P -> I`, which implies unavoidable interpretation errors and blurred accountability in some cases.

## Syntax in This Repository

```csharp
using BorowAmpersand.Runtime;

int hp = 100;
var context = borowcontext(
    Name: "battle-loop",
    FutureUsage: "hp will be modified in combat",
    Source: "GameState",
    RespectLevel: 999);

using var scope = BorowExecutionContext.Use(context);

var soul = &borow hp;
*soul -= 30;

var withContext = &borow(hp, context);
var linked = &borow(hp, context, "shared-concept");

static void Heal(borow<int> target)
{
    *target += 20;
}

Heal(&borow hp);
Console.WriteLine((&borow hp).ToString());
```

## Quick Start

```bash
dotnet build BorowAmpersand.sln
dotnet test BorowAmpersand.sln
dotnet run --project demo/BorowAmpersand.Demo/BorowAmpersand.Demo.csproj
```

## Unity Integration

The flow below is aligned with Unity documentation as of **February 20, 2020**.

### 1. Build assemblies

1. Build runtime and generator in `Release`.
2. Keep generator target framework at `netstandard2.0`.
3. Match Roslyn package compatibility required by your Unity version.

### 2. Place DLLs in Unity

1. Add `BorowAmpersand.Runtime.dll` as a normal runtime/plugin assembly.
2. Put generator DLL in a dedicated folder, for example `Assets/Analyzers/`.

### 3. Configure generator DLL in Inspector

1. Select the generator DLL.
2. Disable `Any Platform`.
3. Exclude runtime platforms in `Include Platforms`.
4. Add label `RoslynAnalyzer` (case-sensitive).

## Demo References

- Intentional HP mutation: `demo/BorowAmpersand.Demo/Program.bor.cs`
- Entanglement via shared `conceptId`: `demo/BorowAmpersand.Demo/Program.bor.cs`
