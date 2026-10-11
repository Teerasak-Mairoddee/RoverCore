namespace RoverCore.Control
{
    /// <summary>Ground speed of each side of a differential-drive rover, in metres per second.</summary>
    public readonly struct WheelSpeeds
    {
        public readonly float Left;
        public readonly float Right;

        public WheelSpeeds(float left, float right)
        {
            Left = left;
            Right = right;
        }

        public override string ToString() => $"L={Left:F2} m/s, R={Right:F2} m/s";
    }
}
