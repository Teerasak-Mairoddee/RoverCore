using System;
using NUnit.Framework;
using RoverCore.Control;

namespace RoverCore.Tests.Control
{
    public class DifferentialDriveTests
    {
        private const float Tolerance = 1e-5f;
        private const float TrackWidth = 0.5f;
        private const float MaxWheelSpeed = 2f;

        private DifferentialDrive drive;

        [SetUp]
        public void SetUp() => drive = new DifferentialDrive(TrackWidth, MaxWheelSpeed);

        [Test]
        public void EqualWheelSpeeds_DriveStraight()
        {
            DriveCommand motion = drive.ToBodyMotion(new WheelSpeeds(1f, 1f));

            Assert.That(motion.LinearSpeed, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(motion.AngularSpeed, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void OppositeWheelSpeeds_SpinInPlace()
        {
            DriveCommand motion = drive.ToBodyMotion(new WheelSpeeds(-1f, 1f));

            Assert.That(motion.LinearSpeed, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(motion.AngularSpeed, Is.EqualTo(2f / TrackWidth).Within(Tolerance));
        }

        [Test]
        public void FasterRightSide_TurnsLeft()
        {
            DriveCommand motion = drive.ToBodyMotion(new WheelSpeeds(0.5f, 1f));

            Assert.That(motion.AngularSpeed, Is.GreaterThan(0f));
        }

        [Test]
        public void StoppedLeftSide_PivotsAboutLeftWheel()
        {
            // Pivoting about the left wheel means the centre moves on a circle of
            // radius TrackWidth / 2, so v / ω should equal that radius.
            DriveCommand motion = drive.ToBodyMotion(new WheelSpeeds(0f, 1f));

            Assert.That(motion.LinearSpeed / motion.AngularSpeed, Is.EqualTo(TrackWidth / 2f).Within(Tolerance));
        }

        [TestCase(1f, 0f)]
        [TestCase(0f, 3f)]
        [TestCase(-0.8f, 1.2f)]
        public void InverseThenForward_ReturnsOriginalCommand(float linear, float angular)
        {
            var command = new DriveCommand(linear, angular);

            DriveCommand roundTrip = drive.ToBodyMotion(drive.ToWheelSpeeds(command));

            Assert.That(roundTrip.LinearSpeed, Is.EqualTo(linear).Within(Tolerance));
            Assert.That(roundTrip.AngularSpeed, Is.EqualTo(angular).Within(Tolerance));
        }

        [Test]
        public void Limit_WithinLimits_LeavesSpeedsUnchanged()
        {
            WheelSpeeds limited = drive.Limit(new WheelSpeeds(1.5f, -2f));

            Assert.That(limited.Left, Is.EqualTo(1.5f).Within(Tolerance));
            Assert.That(limited.Right, Is.EqualTo(-2f).Within(Tolerance));
        }

        [Test]
        public void Limit_TooFast_ScalesBothSidesAndKeepsTheirRatio()
        {
            WheelSpeeds limited = drive.Limit(new WheelSpeeds(1f, 4f));

            Assert.That(limited.Right, Is.EqualTo(MaxWheelSpeed).Within(Tolerance));
            Assert.That(limited.Left, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void Saturate_FullThrottleAndFullTurn_KeepsTurningInSameDirection()
        {
            DriveCommand saturated = drive.Saturate(new DriveCommand(MaxWheelSpeed, 4f));

            Assert.That(saturated.LinearSpeed, Is.LessThan(MaxWheelSpeed));
            Assert.That(saturated.AngularSpeed, Is.GreaterThan(0f));
            WheelSpeeds wheels = drive.ToWheelSpeeds(saturated);
            Assert.That(Math.Max(Math.Abs(wheels.Left), Math.Abs(wheels.Right)),
                Is.LessThanOrEqualTo(MaxWheelSpeed + Tolerance));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Constructor_InvalidTrackWidth_Throws(float trackWidth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DifferentialDrive(trackWidth, MaxWheelSpeed));
        }
    }
}
