# RoverCore Learning Log

## Profile (Milestone 0, 2026-10-11)

- **Background:** experienced C#/Unity developer (FPS games, procedural animation, ballistics). Single-player game work; robotics concepts are the new ground.
- **Tools:** Windows, Visual Studio, Unity 6000.6.5f1 (from `ProjectSettings/ProjectVersion.txt`), URP 17.6.
- **Learning priority:** fundamentals first.
- **Implication:** skip Unity/C# basics; spend explanation time on robotics and engineering concepts (kinematics, occupancy grids, A*, sensing, state machines, testing, evaluation).

## Decisions

| Date | Decision | Why |
|---|---|---|
| 2026-10-11 | Keep navigation core as engine-free C# in its own assembly | Fast Edit Mode tests; enforces world/belief separation |
| 2026-10-11 | Public GitHub repo `RoverCore` | Portfolio visibility |
| 2026-10-11 | Differential-drive movement model (Tee's choice) | Matches real ground robots; gives Milestone 3 realistic control inputs |

## Concepts covered

### Milestone 1 (in progress)

- **Differential-drive kinematics:** `v = (vR + vL)/2`, `ω = (vR − vL)/W`, and the inverse. See `DifferentialDrive.cs`.
- **Kinematic vs dynamic models:** we command speeds and rate-limit them, rather than simulating torque and friction.
- **Saturation that preserves curvature:** when a side is too fast, scale *both* sides by the same factor, so the rover follows the same arc more slowly.
- **Non-holonomic constraint:** a wheeled rover can't move sideways. The controller removes any sideways velocity each physics step.
- **Coordinate conventions:** robotics (ROS REP-103) uses counter-clockwise-positive yaw; Unity's +Y rotation is clockwise from above. We convert once, at the controller boundary.
- **Engine-free core assembly:** `RoverCore.Core` has `noEngineReferences`, so the compiler stops Unity types leaking into logic that should be testable on its own.

## Problems and fixes

_(none yet)_

## Questions to revisit

- UI Toolkit vs Canvas UI (Milestone 5).

## Exercises

- **M1:** before running anything, predict what the rover does when `vL = −vR`, and when `vL = 0`. Check your answers against `OppositeWheelSpeeds_SpinInPlace` and `StoppedLeftSide_PivotsAboutLeftWheel`.
- **M1:** add a "tank mode" toggle to `ManualDriveInput` that sets `vL`/`vR` directly from Q/A and E/D. Hint: go through `DifferentialDrive.ToBodyMotion`.
