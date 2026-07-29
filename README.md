# Root Pattern for Unity

Small framework for creating an application from a composition root.

`Root<TContext>` is a plain C# class. It has no update loop and exposes only two
lifecycle operations: `Initialize()` and `Dispose()`. It composes an object graph
from an explicit, strongly typed context; it does not own a tree of child roots.

## Assemblies

- `Root` — the framework, including the optional `RootBehaviour` Unity adapter.
- `Root.Example` — working examples of the plain C# and `MonoBehaviour` entry points.

## Core usage

```csharp
var context = new GameRootContext(camera, gameConfig, new GameLog());

using var root = new GameRoot(context);
root.Initialize();
```

Every root declares its required context type:

```csharp
public sealed class GameRoot : Root<GameRootContext>
{
    public GameRoot(GameRootContext context) : base(context) { }
}
```

Contexts are `struct` types implementing `IRootContext`. They make dependencies
explicit and prevent the framework from becoming a service locator.

## Unity usage

Derive a scene or prefab component from `RootBehaviour`, serialize a concrete
context struct in it, and create the plain C# root in `CreateRoot`.

```csharp
public sealed class GameRootBehaviour : RootBehaviour
{
    [SerializeField] private GameRootContext _context;

    protected override IRoot CreateRoot() => new GameRoot(_context);

    private void Awake() => InitializeRoot();
}
```

The choice of Unity callback is application code, not part of the root API.
`RootBehaviour` disposes the created root in `OnDestroy`.

Unity serializes only Unity-supported fields of a context. Runtime-only
dependencies can be supplied through its constructor or by returning a copied
context from a method such as `WithLog` before creating the root.
