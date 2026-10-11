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
- **`FixedUpdate` for actuation, `Update` for input:** physics steps on a fixed 0.02 s clock, so the controller's acceleration limit (`maxLinearAcceleration * fixedDeltaTime`) only means 2 m/s² if it runs once per physics step. Input is refreshed per rendered frame, so `ManualDriveInput` reads it in `Update` and only stores a target; it never touches the Rigidbody. Robot terms: the operator sends commands whenever it likes, the motor control loop applies the latest one at a fixed rate.
- **Transform, Rigidbody, Collider in robot terms:** the Transform is the pose `(x, z, heading)`; the Rigidbody is the chassis as a mass that the motor controller acts on (mass, velocity, constraints), and it writes the pose back each step; the Collider is the rover's footprint for contact, separate from the visible mesh, and carries the surface friction material.
- **Turn radius from wheel speeds:** `R = (W/2) · (vR + vL) / (vR − vL)`. The difference between the sides sets the turn rate, the average sets the travel speed.

### Prediction check (2026-10-11)

| Case | Tee's prediction | What the maths says |
|---|---|---|
| `vL = −vR` | Turns left sharply | `v = 0`, `ω = 2·vR/W`: spins on the spot about its own centre, with no travel. Left only if `vR > 0`. |
| `vL = 0` | Gradual left bend | `v = vR/2`, `ω = vR/W`: pivots about the stopped left wheel at radius `W/2` = 0.35 m, whatever the speed. Tight, not gradual. |

Direction was right both times; the shape was more extreme than predicted. A gradual bend needs both sides moving forward with a small difference, e.g. `vL = 1.6`, `vR = 2.0` gives a 3.15 m radius.

## Problems and fixes

### M1: rover doesn't move, and there are no errors (2026-10-11)

- **Observed:** WASD did nothing and the Console was clean.
- **Hypothesis:** the controller ramped from the *measured* speed by at most 2 m/s² × 0.02 s = 0.04 m/s per step, while sliding friction (0.6 × 9.81 × 0.02 ≈ 0.12 m/s per step) took more away, so the speed stayed at zero.
- **Test before fixing:** in Play mode, Max Linear Acceleration was raised to 20 (0.4 m/s per step). The rover moved, which confirmed the hypothesis.
- **Fix:** ramp from the last *commanded* speed (like a motor controller's setpoint ramp) and keep measured speed for telemetry only. Give the rover collider a zero-friction material, because real wheels roll rather than slide.
- **Regression test:** `RoverControllerTests.ForwardCommand_FromRestWithGroundFriction_DrivesForward` (Play Mode).
- **Lesson:** a "silent" failure is often two forces cancelling out. Do the arithmetic per physics step.
- **Trade-off:** after hitting a wall, the commanded speed stays at the target, so the rover keeps pushing until the command changes. Collision policy is handled in Milestone 4.

## Questions to revisit

- UI Toolkit vs Canvas UI (Milestone 5).

## Exercises

- **M1:** before running anything, predict what the rover does when `vL = −vR`, and when `vL = 0`. Check your answers against `OppositeWheelSpeeds_SpinInPlace` and `StoppedLeftSide_PivotsAboutLeftWheel`.
- **M1:** add a "tank mode" toggle to `ManualDriveInput` that sets `vL`/`vR` directly from Q/A and E/D. Hint: go through `DifferentialDrive.ToBodyMotion`.
