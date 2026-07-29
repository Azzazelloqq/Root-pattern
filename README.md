# Root Pattern for Unity

> A small, explicit Composition Root framework for Unity applications.

`Root` helps create an application object graph from one clearly defined entry
point. It keeps dependencies visible, works with both pure C# and Unity scene
objects, and has no hidden update loop or service locator.

## Why Root?

Complex Unity projects often spread creation logic between `MonoBehaviour`s,
singletons and static accessors. Root makes the composition boundary explicit:

```text
Scene / prefab                         Pure C# entry point
      │                                        │
      ▼                                        ▼
RootBehaviour ── serialized context ──► Root<TContext>
                                               │
                                               ▼
                                   application object graph
```

The root creates and wires the objects needed by an application feature. The
objects themselves receive explicit constructor arguments; the framework never
looks dependencies up by type at runtime.

## Principles

| Principle | Meaning |
| --- | --- |
| Explicit dependencies | Every root receives one strongly typed context. |
| Pure C# core | `Root<TContext>` does not inherit from `MonoBehaviour`. |
| Unity adapter | `RootBehaviour` is only an optional bridge from a scene or prefab. |
| Controlled lifetime | A root has `Initialize()`, `Dispose()` and one cancellation token. |
| No hidden architecture | No update loop, global root, dependency container or automatic child-root tree. |

## Installation

### Git submodule

```bash
git submodule add https://github.com/Azzazelloqq/Root-pattern.git Assets/Root
```

### Unity Package Manager

Add the dependency to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.azzazello.root": "https://github.com/Azzazelloqq/Root-pattern.git"
  }
}
```

The package supports Unity `2020.3` and newer.

## Quick start: pure C# root

First, describe exactly what the root needs. A context must be a `struct` that
implements `IRootContext`.

```csharp
using RootPattern;

public interface IGameLog
{
    void Write(string message);
}

public readonly struct GameContext : IRootContext
{
    public GameContext(IGameLog log, string playerName)
    {
        Log = log;
        PlayerName = playerName;
    }

    public IGameLog Log { get; }
    public string PlayerName { get; }
}
```

Create a root from that context:

```csharp
using RootPattern;

public sealed class GameRoot : Root<GameContext>
{
    public GameRoot(GameContext context) : base(context)
    {
    }

    protected override void OnInitialize()
    {
        Context.Log.Write($"Welcome, {Context.PlayerName}!");
    }
}
```

Compose and run it at the application boundary:

```csharp
var context = new GameContext(new ConsoleGameLog(), "Player");

using var root = new GameRoot(context);
root.Initialize();
```

## Unity scene or prefab entry point

Unity can serialize a concrete context struct containing supported Unity fields.
The `MonoBehaviour` creates the regular C# root; it does not contain application
logic itself.

```csharp
using System;
using RootPattern;
using UnityEngine;

[Serializable]
public struct GameSceneContext : IRootContext
{
    [SerializeField] private Camera _camera;
    [SerializeField] private string _playerName;
    [NonSerialized] private IGameLog _log;

    public Camera Camera => _camera;
    public string PlayerName => _playerName;
    public IGameLog Log => _log;

    private GameSceneContext(Camera camera, string playerName, IGameLog log)
    {
        _camera = camera;
        _playerName = playerName;
        _log = log;
    }

    public GameSceneContext WithLog(IGameLog log)
    {
        return new GameSceneContext(_camera, _playerName, log);
    }
}

public sealed class GameSceneRoot : Root<GameSceneContext>
{
    public GameSceneRoot(GameSceneContext context) : base(context)
    {
    }

    protected override void OnInitialize()
    {
        Context.Log.Write($"Welcome, {Context.PlayerName}!");
    }
}

public sealed class GameRootBehaviour : RootBehaviour
{
    [SerializeField] private GameSceneContext _context;

    private void Awake()
    {
        InitializeRoot();
    }

    protected override IRoot CreateRoot()
    {
        return new GameSceneRoot(_context.WithLog(new UnityGameLog()));
    }

    private sealed class UnityGameLog : IGameLog
    {
        public void Write(string message)
        {
            Debug.Log(message);
        }
    }
}
```

`RootBehaviour` calls `Dispose()` on the created root from `OnDestroy`. The
choice of when to call `InitializeRoot()` is application code: `Awake`, `Start`,
a custom bootstrapper, or any other appropriate entry point.

### Runtime-only dependencies

Unity cannot serialize interfaces and arbitrary managed objects. Keep serialized
references in the context, then create a copy enriched with runtime-only
dependencies before constructing the root. Replace `CreateRoot` above with:

```csharp
protected override IRoot CreateRoot()
{
    var runtimeContext = _context.WithLog(new UnityGameLog());
    return new GameSceneRoot(runtimeContext);
}
```

`Root.Example` contains this complete scenario in
[`ExampleRootContext`](Example/ExampleContracts.cs),
[`ExampleRootBehaviour`](Example/ExampleRootBehaviour.cs) and
[`PlainRootExample`](Example/PlainRootExample.cs).

## Lifecycle and cancellation

Each root follows a small, explicit lifecycle:

```text
Created ── Initialize() ──► Initialized ── Dispose() ──► Disposed
                │
                └──► InitializationFailed
```

`IRoot.CancellationToken` belongs to the root lifetime. It is cancelled when
initialization fails and immediately before `OnDispose()` runs.

```csharp
protected override void OnInitialize()
{
    _operation = LoadDataAsync(CancellationToken);
}

protected override void OnDispose()
{
    // CancellationToken is already cancelled here.
    // Release only the resources this root created directly.
}
```

`Dispose()` is safe to call repeatedly. Calling `Initialize()` more than once
is treated as an error.

## What Root deliberately does not do

- It is not a dependency injection container.
- It does not expose `Get<T>()` or a global service locator.
- It does not manage an `Update`, `Tick` or player loop.
- It does not own or automatically dispose child roots.
- It does not dictate Unity callback timing.

Those decisions stay visible in your application code.

## Project layout

```text
Root-pattern/
├── Source/
│   ├── IRoot.cs
│   ├── IRootContext.cs
│   ├── Root.cs
│   ├── RootBehaviour.cs
│   └── Root.asmdef
└── Example/
    ├── ExampleRootContext and usage samples
    └── Root.Example.asmdef
```

## API summary

```csharp
public interface IRoot : IDisposable
{
    CancellationToken CancellationToken { get; }
    void Initialize();
}

public abstract class Root<TContext> : IRoot
    where TContext : struct, IRootContext
{
    protected Root(TContext context);
    protected TContext Context { get; }

    public RootState State { get; }
    public CancellationToken CancellationToken { get; }

    public void Initialize();
    public void Dispose();

    protected abstract void OnInitialize();
    protected virtual void OnDispose();
}
```
