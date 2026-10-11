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

_(filled in as milestones progress)_

## Problems and fixes

_(none yet)_

## Questions to revisit

- UI Toolkit vs Canvas UI (Milestone 5).

## Exercises

_(added per milestone)_
