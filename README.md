# Root Pattern for Unity

> A small, explicit lifecycle base for Unity application composition roots.

`Root` defines the lifetime of an application object graph at one explicit entry
point. Dependencies stay visible through constructors: the package has no
global context, service locator, container, or hidden update loop.

## Why Root?

Complex Unity projects often spread creation logic between `MonoBehaviour`s,
singletons, and static accessors. A root keeps the composition boundary
explicit:

```text
Application entry point
          |
          v
        Root
          |
          v
application object graph
```

The root creates and wires the objects needed by an application feature. Those
objects receive their dependencies through constructors; nothing is resolved by
type at runtime.

## Principles

| Principle | Meaning |
| --- | --- |
| Explicit dependencies | Each concrete root declares dependencies in its constructor. |
| Pure C# | `Root` does not inherit from `MonoBehaviour` or depend on Unity APIs. |
| Controlled lifetime | A root has `InitializeAsync()`, `Dispose()`, and one cancellation token. |
| No hidden architecture | No global root, dependency container, update loop, or child-root tree. |

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

## Quick start

Declare the dependencies of a concrete root directly in its constructor:

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RootPattern;

public interface IGameLog
{
    void Write(string message);
}

public sealed class GameRoot : Root
{
    private readonly IGameLog _log;
    private readonly string _playerName;

    public GameRoot(IGameLog log, string playerName)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _playerName = playerName ?? throw new ArgumentNullException(nameof(playerName));
    }

    protected override UniTask OnInitializeAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _log.Write($"Welcome, {_playerName}!");
        return default;
    }
}
```

Compose and run the root at an application boundary:

```csharp
using var root = new GameRoot(new ConsoleGameLog(), "Player");
await root.InitializeAsync(CancellationToken.None);
```

In Unity, create the root from the lifecycle component or bootstrapper that
owns the application entry point. The package deliberately does not prescribe
the Unity callback or provide a `MonoBehaviour` adapter.

`InitializeAsync()` and `OnInitializeAsync()` return `UniTask`.

## Lifecycle and cancellation

Each root follows a small, explicit lifecycle:

```text
Created -- InitializeAsync() --> Initialized -- Dispose() --> Disposed
              |
              +--> InitializationFailed
```

The token passed to `InitializeAsync()` is forwarded to `OnInitializeAsync()`.
`Root.CancellationToken` belongs to the root lifetime and is cancelled when
initialization fails and immediately before `OnDispose()` runs.

```csharp
protected override async UniTask OnInitializeAsync(CancellationToken token)
{
    await LoadDataAsync(token);
}

protected override void OnDispose()
{
    // CancellationToken is already cancelled here.
    // Release only resources this root created directly.
}
```

`Dispose()` is safe to call repeatedly. Calling `InitializeAsync()` more than once
is an error.

## What Root deliberately does not do

- It is not a dependency injection container.
- It has no global context or service locator.
- It does not manage an `Update`, `Tick`, or player loop.
- It does not own or automatically dispose child roots.
- It does not dictate Unity callback timing or provide a `MonoBehaviour` adapter.

Those decisions stay visible in application code.

## Project layout

```text
Root-pattern/
|-- Source/
|   |-- Root.cs
|   |-- RootState.cs
|   `-- Root.asmdef
`-- Example/
    |-- ExampleApplicationRoot.cs
    |-- ExampleContracts.cs
    |-- PlainRootExample.cs
    `-- Root.Example.asmdef
```

## API summary

```csharp
public abstract class Root : IDisposable
{
    public RootState State { get; }
    public CancellationToken CancellationToken { get; }

    public UniTask InitializeAsync(CancellationToken token);
    public void Dispose();

    protected abstract UniTask OnInitializeAsync(CancellationToken token);
    protected virtual void OnDispose();
}
```
