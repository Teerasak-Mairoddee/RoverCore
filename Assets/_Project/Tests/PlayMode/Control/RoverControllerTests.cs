using System.Collections;
using NUnit.Framework;
using RoverCore.Control;
using RoverCore.Runtime.Control;
using UnityEngine;
using UnityEngine.TestTools;

namespace RoverCore.Tests.Control
{
    /// <summary>
    /// Drives a bare rover on a plain ground plane. Both use Unity's default friction
    /// on purpose: the first version of the controller couldn't overcome it.
    /// </summary>
    public class RoverControllerTests
    {
        private GameObject ground;
        private GameObject rover;
        private RoverController controller;

        [SetUp]
        public void SetUp()
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.localScale = new Vector3(5f, 1f, 5f);

            rover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rover.transform.position = new Vector3(0f, 0.5f, 0f);
            rover.AddComponent<Rigidbody>().mass = 50f;
            controller = rover.AddComponent<RoverController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(rover);
            Object.Destroy(ground);
        }

        private static IEnumerator Simulate(float seconds)
        {
            int steps = Mathf.CeilToInt(seconds / Time.fixedDeltaTime);
            for (int i = 0; i < steps; i++)
                yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator ForwardCommand_FromRestWithGroundFriction_DrivesForward()
        {
            controller.SetCommand(new DriveCommand(1f, 0f));

            yield return Simulate(2f);

            Assert.That(rover.transform.position.z, Is.GreaterThan(1f), "Rover should have covered at least 1 m.");
            Assert.That(controller.ForwardSpeed, Is.GreaterThan(0.5f));
        }

        [UnityTest]
        public IEnumerator TurnCommand_FromRest_TurnsLeft()
        {
            controller.SetCommand(new DriveCommand(0f, 1f));

            yield return Simulate(0.5f);

            // Heading is clockwise from +Z, so a left turn takes it below 360.
            Assert.That(controller.HeadingDegrees, Is.InRange(270f, 359f));
        }

        [UnityTest]
        public IEnumerator StopCommand_WhileMoving_ComesToRest()
        {
            controller.SetCommand(new DriveCommand(1f, 0f));
            yield return Simulate(1.5f);

            controller.Stop();
            yield return Simulate(1.5f);

            Assert.That(Mathf.Abs(controller.ForwardSpeed), Is.LessThan(0.05f));
        }
    }
}
