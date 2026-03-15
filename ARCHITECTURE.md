# Architecture

## Overview

Resolver Engine is an **Entity-Behaviour** engine built with .NET 9 and C#. Entities are lightweight identifiers; behaviours define data contracts as interfaces; module workers provide concrete storage and logic for those contracts. Communication between the backend and module runtimes uses **NATS** as the message transport with **MessagePack** serialization. A Roslyn-based source generator (**Engine.Generators**) eliminates boilerplate by generating client-side NATS proxies and worker-side dispatch code for behaviour interfaces.

## Solution Layout

```
Engine.sln
├── src/                         # Core libraries and executables
│   ├── Engine.Core              # Shared contracts (interfaces, value types)
│   ├── Engine.Client            # Client-side proxies (Entity, World)
│   ├── Engine.Generators        # Roslyn source generator (analyzer)
│   ├── Engine.Module            # Module-side abstractions (BehaviourWorker, IDataDispatch)
│   ├── Engine.ModuleRuntime     # Executable host for running modules
│   ├── Engine.Sandbox           # Console app for experimentation
│   └── Engine.Backend           # Central backend process
└── modules/                     # Pluggable module implementations
    ├── Modules.InMemoryPose     # In-memory IPose behaviour worker
    └── Modules.InMemoryParent   # In-memory IParent behaviour worker
```

### Planned / Referenced (not yet implemented)

- **Engine.Math** — Math utilities, referenced by Modules.InMemoryPose.
- **Engine.Hierarchy** — Hierarchy utilities, referenced by Modules.InMemoryParent.

## Build & Tooling

| Setting | Value |
|---|---|
| SDK | .NET 9 (`global.json` → `9.0.100`, `rollForward: latestFeature`) |
| Target framework | `net9.0` (set in `Directory.Build.props`) |
| Nullable reference types | Enabled globally |
| Implicit usings | Enabled globally |
| Warnings as errors | Enabled globally |
| Package management | Central (`Directory.Packages.props`) |

### Central Package Versions

| Package | Version | Used by |
|---|---|---|
| `MessagePack` | 3.1.4 | Engine.Backend, Engine.ModuleRuntime, module projects |
| `NATS.Net` | 2.7.2 | Engine.Backend, Engine.Module, Engine.ModuleRuntime |
| `Microsoft.CodeAnalysis.CSharp` | 4.12.0 | Engine.Generators |
| `Microsoft.CodeAnalysis.Analyzers` | 3.3.4 | Engine.Generators |

### VS Code Tasks

- **build** — `dotnet build Engine.sln`
- **publish** — `dotnet publish Engine.sln`
- **watch** — `dotnet watch run` (solution-level)

## Key Concepts

### Entity

An entity is a lightweight identity represented by `EntityId` (a `readonly record struct` wrapping a `Guid`). The `Entity` class in Engine.Client associates an `EntityId` with methods to add, remove, query, and retrieve behaviours.

### EntityRepository

`EntityRepository` (Engine.Backend) is the central in-memory store for entity existence and per-entity behaviour sets. Used by `EntityService`, providing a single source of truth for:

- Entity lifecycle — `Create`, `Destroy`, `Exists`, `ListAll`.
- Behaviour tracking — `AddBehaviour`, `RemoveBehaviour`, `HasBehaviour`, `ListBehaviours`.

All operations are thread-safe via `ConcurrentDictionary`.

### Behaviour

A **behaviour** is a data contract defined as an interface in Engine.Core.

- `IBehaviour` — marker interface; all behaviours implement this.
- `IDataBehaviour<T> : IBehaviour` — a convenience base for behaviours that hold typed data with async `GetDataAsync` and `SetDataAsync` methods.
- `IPose : IDataBehaviour<Pose>` — position and rotation (`Vector3` + `Quaternion`).
- `IParent : IDataBehaviour<EntityId>` — parent-child entity relationship.

Behaviours are **interfaces only**; they carry no implementation. Any interface extending `IBehaviour` can define arbitrary async methods (returning `Task` or `Task<T>`, with zero or one value parameter plus an optional `CancellationToken`). The source generator produces client-side proxies and worker-side dispatch code for all methods declared on a behaviour interface.

### BehaviourWorker

`BehaviourWorker<T>` (Engine.Module) is the abstract base class for module workers. It is generic over a behaviour interface `T : IBehaviour` and provides:

- `EntityId` property — set by the module runtime after construction to identify which entity this worker belongs to.
- `OnAddedAsync(CancellationToken)` — called when the behaviour is attached to an entity.
- `OnRemovedAsync(CancellationToken)` — called when the behaviour is removed.

Concrete workers (e.g., `InMemoryPoseWorker`, `InMemoryParentWorker`) extend this base and implement the methods from their behaviour interface. One worker instance is created per `(EntityId, behaviour)` pair.

### IDataDispatch

`IDataDispatch` (Engine.Module) is a non-generic interface implemented by generated worker partial classes:

```csharp
public interface IDataDispatch
{
    Task<ReadOnlyMemory<byte>> DispatchAsync(string methodName, ReadOnlyMemory<byte> payload, CancellationToken ct);
}
```

It provides a single entry point for the ModuleRuntime to invoke any behaviour method on a worker without reflection. The source generator emits a `switch` over method names, deserializes parameters with MessagePack, calls the worker's concrete method, and serializes the return value.

### Source Generator (Engine.Generators)

`BehaviourProxyGenerator` is a Roslyn `IIncrementalGenerator` (`netstandard2.0`, referenced as an analyzer) that produces two kinds of generated code:

- **Worker-side partial classes** — for each `partial` class inheriting `BehaviourWorker<T>`, the generator emits a partial that adds the behaviour interface to the class declaration and implements `IDataDispatch` with a method dispatch switch.
- **Client-side proxy classes** — for each interface extending `IBehaviour`, the generator emits a proxy class (e.g., `PoseProxy` for `IPose`) that implements the behaviour interface and forwards each method call over NATS request-reply to the ModuleRuntime.

Proxy classes accept an `EntityId` and `INatsConnection` and can be obtained via `Entity.GetBehaviour<T>()`.

### World

`World` (Engine.Client) is the client-side proxy to the backend `EntityService`. It accepts an `INatsConnection` and forwards entity lifecycle operations over NATS request-reply:

- `CreateEntityAsync` → `entity.create` — returns a local `Entity` handle.
- `DestroyEntityAsync` → `entity.destroy` — removes an entity from the backend.
- `EntityExistsAsync` → `entity.exists` — checks if an entity exists.
- `ListEntitiesAsync` → `entity.list` — returns all known entity IDs.

## Project Dependency Graph

```
Engine.Core  (no dependencies)
    ↑
Engine.Generators  ──packages──▶ Microsoft.CodeAnalysis.CSharp

Engine.Client  ──references──▶ Engine.Core
               ──packages────▶ NATS.Net
    ↑
Engine.Module  ──references──▶ Engine.Core, Engine.Client
    ↑
Engine.ModuleRuntime  ──references──▶ Engine.Core, Engine.Module
                      ──packages────▶ NATS.Net, MessagePack

Engine.Sandbox  ──references──▶ Engine.Core, Engine.Client
                ──packages────▶ NATS.Net

Engine.Backend  ──references──▶ Engine.Core
                ──packages────▶ NATS.Net, MessagePack

Modules.InMemoryPose   ──references──▶ Engine.Core, Engine.Module, Engine.Math (planned)
                       ──analyzer────▶ Engine.Generators
                       ──packages────▶ MessagePack
Modules.InMemoryParent ──references──▶ Engine.Core, Engine.Module, Engine.Hierarchy (planned)
                       ──analyzer────▶ Engine.Generators
                       ──packages────▶ MessagePack
```

## Transport & Serialization

- **NATS** (`NATS.Net` package) is the messaging backbone connecting the backend to module runtimes.
- **MessagePack** is the wire format for behaviour data exchanged over NATS.
- The Engine.ModuleRuntime process hosts module workers and bridges NATS messages to `BehaviourWorker` lifecycle methods.

## Process Model

Two executable projects exist:

1. **Engine.Backend** — the central server process. Hosts the `EntityService` (entity lifecycles and behaviour tracking) over NATS. Acts as a two-phase orchestrator: when a behaviour is added or removed, the backend first sends a NATS request to the module runtime and only commits the change to the entity registry if the runtime responds successfully.
2. **Engine.ModuleRuntime** — the module host process. Connects to NATS, discovers module DLLs, builds a type registry of `BehaviourWorker<T>` types, and subscribes to `worker.create.<name>` and `worker.remove.<name>` subjects to create/destroy worker instances on demand. Additionally subscribes to `behaviour.<name>.*` subjects to dispatch behaviour method calls to live workers via their `IDataDispatch` implementation.

### Behaviour Add Flow

1. A module calls `Entity.AddBehaviourAsync<T>()`, which sends a request to `entity.add-behaviour`.
2. The backend validates the request (entity exists, behaviour not already added).
3. The backend sends a NATS request to `worker.create.<behaviourName>` with the `EntityId` as payload.
4. The module runtime creates a new `BehaviourWorker<T>` instance, sets its `EntityId` property, calls `OnAddedAsync`, and replies `"ok"`.
5. On success, the backend registers the behaviour in the `EntityRepository` and replies `"ok"` to the caller.
6. On failure (no responders, timeout, or error), the backend replies with an error and does **not** register the behaviour.

### Entity Destroy Flow

1. A client calls `World.DestroyEntityAsync(id)`, which sends a request to `entity.destroy`.
2. The backend removes the entity from the `EntityRepository`, obtaining the list of behaviours that were attached.
3. For each behaviour, the backend sends a `worker.remove.<behaviourName>` request to the module runtime, which calls `OnRemovedAsync` on the worker and removes it.
4. The backend replies `"ok"` to the caller.

### Behaviour Remove Flow

Same two-phase pattern: `entity.remove-behaviour` → `worker.remove.<behaviourName>` → `OnRemovedAsync` → remove from repository.

### Module Loading

At startup the ModuleRuntime scans `{AppContext.BaseDirectory}/modules/` for `.dll` files. For each assembly it finds, it reflects over exported types and builds a **type registry** (`Dictionary<string, Type>`) mapping each behaviour name (e.g. `"IPose"`) to the concrete `BehaviourWorker<T>` type that handles it. No worker instances are created eagerly — they are instantiated on demand when `worker.create.<name>` requests arrive.

Workers are created via parameterless constructors (`Activator.CreateInstance`). Live instances are tracked in a dictionary keyed by `(EntityId, behaviourName)` so they can be looked up for removal.

To deploy a module, copy its build output (DLL + dependencies) into the `modules/` sub-directory of the ModuleRuntime publish output.

Modules run inside the ModuleRuntime process, not as separate executables.

## NATS Subject Conventions

All service endpoints are exposed via NATS micro-services (`NatsSvcServer`). Subjects follow the pattern `<service>.<operation>`.

### EntityService (`entity`)

| Subject | Request | Reply | Description |
|---|---|---|---|
| `entity.create` | empty | EntityId (Guid string) | Create a new entity |
| `entity.destroy` | EntityId (Guid string) | `"ok"` or error | Destroy an existing entity |
| `entity.exists` | EntityId (Guid string) | `"true"` / `"false"` | Check if an entity exists |
| `entity.list` | empty | comma-separated EntityIds | List all entity IDs |
| `entity.add-behaviour` | `entityId:behaviourName` | `"ok"` or error | Add a behaviour to an entity (triggers `worker.create`) |
| `entity.remove-behaviour` | `entityId:behaviourName` | `"ok"` or error | Remove a behaviour from an entity (triggers `worker.remove`) |
| `entity.has-behaviour` | `entityId:behaviourName` | `"true"` / `"false"` or error | Check if an entity has a behaviour |
| `entity.list-behaviours` | EntityId (Guid string) | comma-separated behaviour names | List behaviours on an entity |

### ModuleRuntime (worker lifecycle)

| Subject | Request | Reply | Description |
|---|---|---|---|
| `worker.create.<behaviourName>` | EntityId (Guid string) | `"ok"` or error | Create a worker instance for the given entity and behaviour |
| `worker.remove.<behaviourName>` | EntityId (Guid string) | `"ok"` or error | Remove the worker instance for the given entity and behaviour |

### Behaviour Method Dispatch (`behaviour`)

| Subject | Request | Reply | Description |
|---|---|---|---|
| `behaviour.<behaviourName>.<methodName>` | EntityId (Guid string) or MessagePack parameter (with EntityId in `EntityId` header) | MessagePack result or `"ok"` | Invoke a behaviour method on the worker for the given entity |

When a method has no value parameter, the `EntityId` is sent as a Guid string in the request payload. When a method has one value parameter, the parameter is serialized as MessagePack in the payload and the `EntityId` is sent in a NATS header named `EntityId`.

Examples: `behaviour.IPose.GetDataAsync`, `behaviour.IPose.SetDataAsync`, `behaviour.IParent.GetDataAsync`.

Errors are returned via NATS service error replies with a numeric code and description, or as plain string error messages from the module runtime.

## Conventions

- All behaviour interfaces live in **Engine.Core** so they can be shared between backend and module code without circular dependencies.
- Module projects live under the `modules/` folder and reference Engine.Core + Engine.Module.
- Module worker classes are `partial` to support source generation.
- Async-first API: all behaviour and entity operations return `Task` and accept `CancellationToken`.
- Behaviour method constraints: must return `Task` or `Task<T>`, accept 0 or 1 value parameter plus optional `CancellationToken`.
- Generated proxy naming convention: interface name with leading `I` stripped plus `Proxy` suffix (e.g., `IPose` → `PoseProxy`).
- `Entity.GetBehaviour<T>()` resolves proxy types by naming convention at runtime.
