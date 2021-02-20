# Borow Ampersand Theory (BAT) - Implementation Contract

## 1. Operator Model
- Operator form: `&borow x`
- Extended operator form: `&borow(x, context)` and `&borow(x, context, conceptId)`
- Type sugar form: `borow<T>` -> `BorowRef<T>`
- Context sugar form: `borowcontext(...)` -> `new BorowContext(...)`
- Runtime equivalent: `Borow.Ampersand(ref x, conceptId: "...")`
- Formal mapping: `&borow : V x C -> (A x Meta)`

`C` is resolved as:
1. explicit `context` argument in `Borow.Ampersand(...)`
2. ambient `BorowExecutionContext.Current`
3. default context

## 2. Meta Contract
`BorowMeta` must expose:
- `ConceptId`
- `VariableName`
- `ContextName`
- `Source`
- `FutureUsage`
- `RespectLevel`
- `IsAware`
- `IsIntentional`
- `ObservedAt`
- `MutatedAt`

## 3. Axioms
- Axiom 1 (Observability): taking `&borow` sets `IsAware = true`.
- Axiom 2 (Intentional Mutability): updates through `BorowRef<T>.Value` set `IsIntentional = true`.
- Axiom 3 (Reference Entanglement): refs with same `ConceptId` synchronize through concept storage.

## 4. Diagnostics
- `BOR001`: `cannot take &borow of temporary object`
  - raised when `&borow` operand is not ref-assignable.
- `BOR404`: `variable '{name}' has no personality`
  - raised when a declared local variable has zero semantic references.
  - `using var` declarations are excluded.

## 5. Generator Pipeline
- `.bor.cs` files are taken from `AdditionalFiles`.
- Custom `&borow` and `borowcontext(...)` syntax is normalized, then validated/re-written via Roslyn syntax + semantic model.
- `*ref` in `.bor.cs` maps to `.Value`.
- `Console.WriteLine(&borow x)` is rewritten to `.ToString()` form for `ref struct` safety.

## 6. Runtime Rules
- Concept store key is `ConceptId` (not raw `file + name` fallback when `ConceptId` is available).
- `IsIntentional` is not reset on repeated observations.
- `Address` is a deterministic hash of `ConceptId`.
