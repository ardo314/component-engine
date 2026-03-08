# Architecture

## Overview

Engine is a distributed Entity-Component framework for .NET 9. It draws loose inspiration from Unity3D's programming model — Entities own Components and Behaviours — but is designed from the ground up as a distributed system where each Behaviour runs as its own service.

All inter-service communication flows through **NATS** (request/reply, pub/sub). All messages are serialized with **MessagePack**. Client and server code is generated at compile time from shared interface definitions using **Roslyn incremental source generators**.

## Solution Structure

```
Engine.sln
├── src/
│   ├── Engine.Core/          # Shared types, contracts, extension interfaces
│   ├── Engine.Runtime/        # Executable host — assembly scanning, NATS, dispatch
│   ├── Engine.Client/         # Client-side API
│   └── Engine.World/          # Extension — entity lifecycle & component management
├── Directory.Build.props      # Shared build settings (net9.0, nullable, warnings-as-errors)
└── Directory.Packages.props   # Central package version management
```

### Engine.Core

The source-of-truth for the system's contract surface. Contains:

- **`EntityId`** — a `readonly record struct` wrapping a `Guid` that uniquely identifies an Entity.
- **`IComponent`** — marker interface for all components (data attached to an Entity).
- **`IBehaviour`** — marker interface for all behaviours (remote logic operating on components).
- **`IEntity`** — contract for entity operations: `AddComponentAsync`, `RemoveComponentAsync`, `GetComponentAsync`, `HasComponentAsync`.
- **`IWorld`** — contract for entity lifecycle: `CreateEntityAsync`, `DestroyEntityAsync`, `GetEntityAsync`.
- **`IExtension`** — entry point for extensions loaded by the runtime. Each extension assembly implements this interface to register its components and behaviours.
- **`IExtensionRegistrar`** — provided by the runtime to extensions during registration. Extensions call `AddComponent<T>()` and `AddBehaviour<TContract, TImpl>()` to declare their types.

Engine.Core has **no dependency** on NATS, MessagePack, or any infrastructure concern. It is a pure contract/types library.

### Engine.Runtime

An executable host process — pure infrastructure, zero domain logic. Responsibilities:

- **Assembly scanning** — `ExtensionLoader` discovers and loads extension DLLs from a configurable directory (default `/app/extensions`), finds all `IExtension` implementations.
- **Extension registration** — `ExtensionRegistrar` (implements `IExtensionRegistrar`) collects component and behaviour type registrations from each extension.
- **NATS connection** — `EngineHost` manages the NATS connection lifecycle.
- **Message dispatch** — routes incoming NATS messages to the appropriate behaviour implementation (planned).
- **MessagePack setup** — aggregates serialization formatters across all loaded extensions (planned).

The runtime is designed to be packaged as a container image. Extensions are baked into the image at build time:

```dockerfile
FROM engine-runtime:latest
COPY MyExtension/bin/Release/net9.0/publish/ /app/extensions/
```

References: `Engine.Core`; Packages: `NATS.Net`, `MessagePack`

### Engine.Client

Developer-facing client API for connecting to a running Engine system.

- **`World`** — implements `IWorld`; communicates with the runtime over NATS.
- **`Entity`** — implements `IEntity`; publishes component operations to NATS subjects.
- **Client proxies** — generated from Engine.Core interfaces by the source generator (planned).

References: `Engine.Core`; Packages: `NATS.Net`, `MessagePack`

### Engine.World

An **extension** — loaded by the runtime like any third-party extension. Provides the core entity/component management system.

- **`WorldExtension`** — implements `IExtension`; registers world-related types with the runtime.
- **`EntityStore`** — thread-safe in-memory storage of `EntityRecord` instances.
- **`EntityRecord`** — holds an entity's identity and its attached `IComponent` instances.
- **`Entity`** — implements `IEntity`; wraps an `EntityRecord` into the core interface.
- **`World`** — implements `IWorld`; manages entity creation, destruction, and lookup.

References: `Engine.Core` only (no NATS, no MessagePack — pure domain logic)

## Extension Model

Extensions are class libraries that reference `Engine.Core` and implement `IExtension`. The runtime discovers them via assembly scanning at startup.

```
/app/extensions/
  ├── Engine.World.dll          # entity lifecycle (ships with Engine)
  ├── Acme.Physics.dll          # third-party physics behaviours
  └── MyGame.Combat.dll         # user's own behaviours
```

From the runtime's perspective, `Engine.World` and third-party extensions are identical — there is no privileged "built-in" code. All domain logic is an extension.

## Key Concepts

| Concept       | Description |
|---------------|-------------|
| **Entity**    | A uniquely identified object in the world. Analogous to Unity's `GameObject`. Has no behaviour of its own; it is a container for Components. |
| **Component** | Data or state attached to an Entity. Can be added and removed at runtime. |
| **Behaviour** | Logic that operates on Components. Each Behaviour is a remote service — its interface lives in Core, its implementation in an extension, and its proxy in Client. |
| **Extension** | A loadable module that registers Components and Behaviours with the runtime via `IExtension`. |

## Communication

**NATS** is the primary transport for all inter-service communication.

- **Request/Reply** — used for operations that return a result (e.g., creating an entity, querying a component).
- **Publish/Subscribe** — used for broadcasting events (e.g., component added, entity destroyed).
- **Subject conventions** — subjects follow the pattern `entity.{id}.{operation}` (e.g., `entity.{id}.component.add.{TypeName}`, `entity.{id}.destroy`). Derived deterministically from entity IDs and component type names.

NATS is installed in the dev container (v2.12.4) for local development.

## Serialization

**MessagePack** is used for all message serialization/deserialization.

- Compact binary format — low overhead for high-frequency messaging.
- All types crossing the wire must be MessagePack-serializable.
- Source-generated formatters are preferred over reflection-based serialization for performance.

## Code Generation

**Roslyn incremental source generators** read the interfaces defined in `Engine.Core` and emit:

1. **Client proxies** (into `Engine.Client`) — each interface method becomes a NATS request serialized with MessagePack.
2. **Server stubs** (into extensions) — base classes or dispatch handlers that deserialize incoming NATS messages and invoke the developer's implementation.

The generator project will be added to the solution as `Engine.Generators` (or similar) when implementation begins.

## Build & Tooling

| Tool / Setting          | Detail |
|-------------------------|--------|
| .NET SDK                | 9.0.100 (`global.json`, `rollForward: latestFeature`) |
| Target framework        | `net9.0` |
| Nullable reference types | Enabled |
| Implicit usings          | Enabled |
| Warnings as errors       | Enabled |
| Package management       | Central (`Directory.Packages.props`) |
| Formatter               | CSharpier 1.2.6 (format on save) |
| Dev container            | `mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim` + NATS server |

### NuGet Packages

Versions are pinned in `Directory.Packages.props`:

| Package     | Version | Used By |
|-------------|---------|---------|
| NATS.Net    | 2.7.2   | Runtime, Client |
| MessagePack | 3.1.4   | Runtime, Client |
