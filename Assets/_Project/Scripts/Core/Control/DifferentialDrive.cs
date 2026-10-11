using System;

namespace RoverCore.Control
{
    /// <summary>
    /// Kinematic model of a differential-drive rover: two independently driven sides
    /// a fixed distance apart, steering by running them at different speeds.
    /// </summary>
    /// <remarks>
    /// Pure C# with no Unity dependency, so it can be unit tested without a scene.
    /// "Kinematic" means it relates speeds to motion and ignores mass, friction and torque.
    /// </remarks>
    public sealed class DifferentialDrive
    {
        /// <summary>Distance between the left and right contact points, in metres.</summary>
        public float TrackWidth { get; }

        /// <summary>Fastest either side can drive, in metres per second.</summary>
        public float MaxWheelSpeed { get; }

        public DifferentialDrive(float trackWidth, float maxWheelSpeed)
        {
            if (!(trackWidth > 0f))
                throw new ArgumentOutOfRangeException(nameof(trackWidth), trackWidth, "Track width must be positive.");
            if (!(maxWheelSpeed > 0f))
                throw new ArgumentOutOfRangeException(nameof(maxWheelSpeed), maxWheelSpeed, "Max wheel speed must be positive.");

            TrackWidth = trackWidth;
            MaxWheelSpeed = maxWheelSpeed;
        }

        /// <summary>Forward kinematics: wheel speeds to body motion.</summary>
        public DriveCommand ToBodyMotion(WheelSpeeds wheels)
        {
            float linear = (wheels.Right + wheels.Left) / 2f;
            float angular = (wheels.Right - wheels.Left) / TrackWidth;
            return new DriveCommand(linear, angular);
        }

        /// <summary>Inverse kinematics: desired body motion to wheel speeds. Not limited.</summary>
        public WheelSpeeds ToWheelSpeeds(DriveCommand command)
        {
            float halfTurn = command.AngularSpeed * TrackWidth / 2f;
            return new WheelSpeeds(command.LinearSpeed - halfTurn, command.LinearSpeed + halfTurn);
        }

        /// <summary>
        /// Keeps both sides within <see cref="MaxWheelSpeed"/>. If either side is too fast,
        /// both are scaled down by the same factor. That keeps the ratio between them, so
        /// the rover follows the same curve, just more slowly. Clamping each side on its
        /// own would quietly change the curve.
        /// </summary>
        public WheelSpeeds Limit(WheelSpeeds wheels)
        {
            float fastest = Math.Max(Math.Abs(wheels.Left), Math.Abs(wheels.Right));
            if (fastest <= MaxWheelSpeed)
                return wheels;

            float scale = MaxWheelSpeed / fastest;
            return new WheelSpeeds(wheels.Left * scale, wheels.Right * scale);
        }

        /// <summary>Returns the closest command the rover can actually achieve.</summary>
        public DriveCommand Saturate(DriveCommand command) =>
            ToBodyMotion(Limit(ToWheelSpeeds(command)));
    }
}
