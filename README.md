# Root Pattern for Unity

Small framework for building an application as an owned tree of roots.

`Root` is a plain C# class. It has no update loop and exposes only two lifecycle
operations: `Initialize()` and `Dispose()`. A parent root owns its child roots:
children initialize after their parent and dispose in reverse creation order.

## Assemblies

- `Root` — the framework, including the optional `RootBehaviour` Unity adapter.
- `Root.Example` — working examples of the plain C# and `MonoBehaviour` entry points.

## Core usage

```csharp
var context = new RootContextBuilder()
    .Register<IGameLog>(new ConsoleGameLog())
    .Register(new GameSettings("Player"))
    .Build();

using var root = new GameRoot(context);
root.Initialize();
```

Dependencies are placed in a context at the composition boundary and consumed
through explicit constructors. The context is not intended to be used as a
service locator throughout application code.

## Unity usage

Derive a scene or prefab component from `RootBehaviour`, serialize Unity
references in it, register those references in `ConfigureContext`, and create
the plain C# root in `CreateRoot`.

```csharp
public sealed class GameRootBehaviour : RootBehaviour
{
    [SerializeField] private PlayerView _playerView;

    protected override void ConfigureContext(RootContextBuilder builder)
    {
        builder.Register<IPlayerView>(_playerView);
        builder.Register(new GameSettings("Player"));
    }

    protected override Root CreateRoot(IRootContext context) => new GameRoot(context);

    private void Awake() => InitializeRoot();
}
```

The choice of Unity callback is application code, not part of the root API.
`RootBehaviour` disposes the created root in `OnDestroy`.

## Context scopes

Each child root receives a child context. It can read parent dependencies and
register its own values. A dependency in a child scope shadows a value of the
same type in its parent scope.
