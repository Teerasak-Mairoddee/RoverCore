# Milestone 1: Unity environment and differential-drive rover

> Status: **in progress**. Plan agreed 2026-10-11. Scripts and Edit Mode tests written; scene and Play Mode checks not yet done.

## A. Objective

A rover you can drive by hand around a small obstacle course, moved by physics and steered like a real ground robot.
Everything after this (waypoint following, replanning) will send the *same* commands to this controller that your keyboard does, so getting the control interface right now pays off in Milestone 3.

## B. Concepts (the new ground for you)

**Differential drive.** Two independently driven sides (wheels or tracks), no steering axle. With left speed `vL`, right speed `vR` and track width `W`:

```
forward speed   v = (vR + vL) / 2          (m/s)
turn rate       ω = (vR − vL) / W          (rad/s, positive = turn left)
```

and the inverse, used when something asks for "go this fast, turn this fast":

```
vL = v − ω·W/2
vR = v + ω·W/2
```

Predict before reading on: what does the rover do when `vL = −vR`? When `vL = 0`?
(Answers in the learning log after you've had a go.)

**Kinematic vs dynamic model.** A *kinematic* model says "the rover moves at the commanded speed" and ignores mass, friction and torque. A *dynamic* model simulates motor torque and tyre friction. Real robot stacks usually plan with a kinematic model and let a lower-level controller deal with dynamics. We'll do the same: kinematic commands, plus acceleration limits so it can't change speed instantly.

**Commands vs actuation.** The controller accepts a `DriveCommand` (target `v`, `ω`). It doesn't know or care whether a keyboard or the autopilot produced it. This is the seam Milestone 3 plugs into.

## C. Design decisions

| Decision | Options | Recommendation | Why |
|---|---|---|---|
| How the Rigidbody moves | (1) `AddForce`/torque with drag, (2) set `linearVelocity`/`angularVelocity` towards the target, rate-limited, (3) kinematic body + `MovePosition`, (4) `WheelCollider`s | **(2)** | Predictable and tunable like a real motor controller, still collides properly. (1) is hard to tune, (3) won't react to collisions, (4) is drivetrain physics, which the spec puts out of scope. |
| Tipping | Free rotation vs freeze X/Z rotation | **Freeze X/Z** for now | Flat terrain, and the navigation stack assumes a 2D pose `(x, z, heading)`. Documented as a limitation. |
| Manual controls | Tank (one key pair per side) vs WASD throttle/turn | **WASD**, mixed into `vL`/`vR` | Exercises the inverse kinematics above; tank mode can be a debug toggle later. |
| Kinematics code | Inside the MonoBehaviour vs separate pure C# class | **Separate** | Lets us Edit-Mode-test the maths with no scene. |

## D. Implementation plan

**Scene (you build this in the Editor, it's quicker by hand than by YAML):**
- `Assets/_Project/Scenes/TestCourse.unity`: 30 × 30 m ground plane, 4–6 box obstacles, a directional light, a follow camera.
- Rover: `Rover` root with `Rigidbody` (≈ 50 kg, interpolate on, X/Z rotation frozen, lowered centre of mass) and one `BoxCollider` (≈ 0.8 × 0.4 × 1.0 m). Wheels are visual only.
- Layers: `Rover`, `Obstacle`, `Ground` (Milestone 4's sensor layer mask depends on these).

**Code (I write a first version, you review and tweak):**

| File | Kind | Responsibility |
|---|---|---|
| `Scripts/Core/Control/DifferentialDrive.cs`, `WheelSpeeds.cs` | pure C# | Forward/inverse kinematics, speed clamping |
| `Scripts/Core/Control/DriveCommand.cs` | struct | Target `v` and `ω` |
| `Scripts/Runtime/Control/RoverController.cs` | MonoBehaviour | Applies commands to the Rigidbody in `FixedUpdate`, with acceleration limits; exposes speed and heading |
| `Scripts/Runtime/Control/ManualDriveInput.cs` | MonoBehaviour | Reads the Input System, produces `DriveCommand`s |
| `Tests/EditMode/Control/DifferentialDriveTests.cs` | test | Straight line, spin in place, pivot on one side, clamping |

Plus assembly definitions `RoverCore.Core` (no Unity references allowed), `RoverCore.Runtime` and `RoverCore.Tests.EditMode` (the Test Framework 1.8.0 is already in your manifest, so no new packages).

**Your exercise:** after review, you add a "tank mode" toggle that drives `vL`/`vR` directly from Q/A and E/D.

## E. Scene setup (Editor steps)

1. **Layers:** Project Settings → Tags and Layers → add `Ground`, `Obstacle`, `Rover`.
2. **Scene:** File → New Scene (Basic URP), save as `Assets/_Project/Scenes/TestCourse.unity`.
3. **Ground:** Plane, scale (3, 1, 3) for 30 × 30 m, layer `Ground`.
4. **Obstacles:** 4–6 Cubes of varied size scattered around, layer `Obstacle`. Leave clear lanes between them.
5. **Rover:** empty `Rover` at (0, 0.3, 0), layer `Rover`.
   - `Rigidbody`: Mass 50, Interpolate, Collision Detection *Continuous*.
   - `BoxCollider`: size (0.8, 0.4, 1.0).
   - Child `Body` cube scaled (0.8, 0.4, 1.0) with its Box Collider **removed** (the root collider is the real one).
   - Optional: four cylinder wheels as children, colliders removed. Something visibly marking the front helps.
   - `RoverController` on the root (defaults are fine).
   - `ManualDriveInput` on the root: drag the root into *Controller*, and `InputSystem_Actions → Player/Move` into *Move Action*.
6. **Camera:** child the Main Camera to the rover at about (0, 3, −5), tilted down ~25°. (A smoothed follow camera can come later.)
7. Save the scene, and make it scene 0 in Build Profiles.

## F. How we'll verify

- Edit Mode tests run green in Test Runner (you run them; I can't run Unity here).
- Manual: drive forward, spin in place, turn into an obstacle and confirm the rover stops against it without tunnelling or tipping.
- You can explain what the Rigidbody, Collider and Transform each contribute to the rover, in robot terms.
