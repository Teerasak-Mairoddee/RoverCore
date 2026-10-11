namespace RoverCore.Control
{
    /// <summary>
    /// A request for how the rover body should move: forward speed and turn rate.
    /// Both the keyboard and (from Milestone 3) the autopilot produce these, so the
    /// controller never needs to know who is driving.
    /// </summary>
    /// <remarks>
    /// Uses the robotics (ROS REP-103) convention: positive <see cref="AngularSpeed"/>
    /// is counter-clockwise seen from above, i.e. a left turn. Unity's Y axis rotates
    /// the other way, so the conversion happens once, in RoverController.
    /// </remarks>
    public readonly struct DriveCommand
    {
        /// <summary>Forward speed in metres per second. Negative drives backwards.</summary>
        public readonly float LinearSpeed;

        /// <summary>Turn rate in radians per second. Positive turns left.</summary>
        public readonly float AngularSpeed;

        public DriveCommand(float linearSpeed, float angularSpeed)
        {
            LinearSpeed = linearSpeed;
            AngularSpeed = angularSpeed;
        }

        public static DriveCommand Stop => new DriveCommand(0f, 0f);

        public override string ToString() => $"v={LinearSpeed:F2} m/s, ω={AngularSpeed:F2} rad/s";
    }
}
