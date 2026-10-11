using RoverCore.Control;
using UnityEngine;

namespace RoverCore.Runtime.Control
{
    /// <summary>
    /// Drives the rover's Rigidbody towards the latest <see cref="DriveCommand"/>,
    /// like a simple motor controller: speed changes are rate-limited, and the body
    /// cannot slide sideways.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RoverController : MonoBehaviour
    {
        [Header("Geometry")]
        [Tooltip("Distance between left and right wheel contact points (m).")]
        [SerializeField, Min(0.01f)] private float trackWidth = 0.7f;

        [Header("Limits")]
        [Tooltip("Top ground speed of either side (m/s).")]
        [SerializeField, Min(0.01f)] private float maxWheelSpeed = 2f;

        [Tooltip("How quickly forward speed can change (m/s²).")]
        [SerializeField, Min(0.01f)] private float maxLinearAcceleration = 2f;

        [Tooltip("How quickly turn rate can change (deg/s²).")]
        [SerializeField, Min(0.01f)] private float maxAngularAccelerationDegrees = 360f;

        private Rigidbody body;
        private DifferentialDrive drive;
        private DriveCommand target = DriveCommand.Stop;

        // The speeds currently being sent to the "motors". They ramp towards the
        // target from their own previous value, not from the measured speed: if they
        // ramped from the measured speed, any resistance larger than the acceleration
        // limit (like ground friction) would hold the rover still forever.
        private float commandedLinear;
        private float commandedAngular;

        /// <summary>The command being tracked, after speed limits were applied.</summary>
        public DriveCommand TargetCommand => target;

        /// <summary>Measured forward speed (m/s). Negative when reversing.</summary>
        public float ForwardSpeed { get; private set; }

        /// <summary>Measured turn rate (rad/s). Positive is a left turn.</summary>
        public float TurnRate { get; private set; }

        /// <summary>Heading in degrees, clockwise from world +Z (like a compass with +Z as north).</summary>
        public float HeadingDegrees => body.rotation.eulerAngles.y;

        /// <summary>Fastest the rover can drive straight (m/s). Useful for scaling inputs.</summary>
        public float MaxWheelSpeed => maxWheelSpeed;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            drive = new DifferentialDrive(trackWidth, maxWheelSpeed);

            // The navigation stack treats the rover as a 2D pose (x, z, heading),
            // so it isn't allowed to pitch or roll. See docs/milestones/M1_PLAN.md.
            body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        /// <summary>Sets the motion to track. Requests beyond the rover's limits are scaled down.</summary>
        public void SetCommand(DriveCommand command)
        {
            target = drive.Saturate(command);
        }

        /// <summary>Ramps down to a standstill using the normal deceleration limit.</summary>
        public void Stop() => SetCommand(DriveCommand.Stop);

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            Vector3 forward = body.rotation * Vector3.forward;

            // Measure what the body is actually doing. This is telemetry only: after
            // a collision, or against friction, it can differ from what was commanded.
            ForwardSpeed = Vector3.Dot(body.linearVelocity, forward);
            // Unity's +Y rotation is clockwise from above; our convention is counter-clockwise.
            TurnRate = -body.angularVelocity.y;

            float maxAngularAcceleration = maxAngularAccelerationDegrees * Mathf.Deg2Rad;
            commandedLinear = Mathf.MoveTowards(commandedLinear, target.LinearSpeed, maxLinearAcceleration * dt);
            commandedAngular = Mathf.MoveTowards(commandedAngular, target.AngularSpeed, maxAngularAcceleration * dt);

            // Wheels don't slide sideways, so velocity is only ever along the heading.
            // Keep the vertical part so gravity still works.
            Vector3 velocity = forward * commandedLinear;
            velocity.y = body.linearVelocity.y;

            body.linearVelocity = velocity;
            body.angularVelocity = new Vector3(0f, -commandedAngular, 0f);
        }
    }
}
