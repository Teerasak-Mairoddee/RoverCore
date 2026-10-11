# RoverCore Architecture (proposed)

> Status: **planned, not implemented**. Each component is created only when a milestone needs it.

## Guiding idea: world vs. belief

The most important split in RoverCore is between **what exists** (the Unity scene) and **what the rover knows** (its `GridMap`).
The planner only ever reads the rover's map. The map is filled from (a) an initial known map and (b) sensor observations.
This mirrors real robotics: a robot never gets the "scene graph", only measurements.

```
          Unity scene (ground truth)
                   │  raycasts only
                   ▼
 SensorSimulator ──observations──▶ NavigationManager ──updates──▶ GridMap (belief)
                                        │   ▲                         │
                         path request   │   │ PathResult              │ IsTraversable(cell)
                                        ▼   │                         ▼
                                     AStarPlanner ◀───────────────────┘
                                        │
                       waypoints (world)│
                                        ▼
                                  RoverController ──forces──▶ Rigidbody
 MissionManager (state) ◀──────events───┘
        │
        ▼
 EventLogger / TelemetryManager ──▶ RoverDashboard
```

## Layers

| Layer | Contents | Depends on Unity? | Tested by |
|---|---|---|---|
| **Core logic** | `GridMap`, `GridCoord`, `AStarPlanner`, `PathResult`, mission state rules | No (plain C#, own asmdef with `noEngineReferences`) | Edit Mode tests |
| **Runtime adapters** | `RoverController`, `SensorSimulator`, `NavigationManager`, `MissionManager` | Yes (MonoBehaviours) | Play Mode tests + manual |
| **Presentation** | `RoverDashboard`, debug gizmos, route drawing | Yes | Manual |

Keeping core logic engine-free means the A* planner and grid can be unit tested in milliseconds and reasoned about without a scene.

## Assemblies (planned)

- `RoverCore.Core` – pure C# navigation logic
- `RoverCore.Runtime` – MonoBehaviours, references Core
- `RoverCore.Tests.EditMode` – references Core
- `RoverCore.Tests.PlayMode` – references Runtime

## Milestones

| # | Goal | Key learning |
|---|---|---|
| 0 | Workspace, repo, plan, learning log | Unity project hygiene, git for Unity |
| 1 | Scene, rover, physics, manual driving | Differential drive kinematics, physics-driven control |
| 2 | Occupancy grid + A* + Edit Mode tests | Grid/world transforms, A*, TDD of algorithms |
| 3 | Autonomous waypoint following | Path → waypoints, heading control, arrival tolerance |
| 4 | Sensors + dynamic replanning | World vs belief, raycast sensing, debounced replanning |
| 5 | Mission states, telemetry, JSON event log | State machines, events vs states, structured logging |
| 6 | Scenarios, tests, metrics, portfolio | Reproducible evaluation, documentation |
