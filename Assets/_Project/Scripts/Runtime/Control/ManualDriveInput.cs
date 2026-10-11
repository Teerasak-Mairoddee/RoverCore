using RoverCore.Control;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoverCore.Runtime.Control
{
    /// <summary>
    /// Turns keyboard or gamepad input into <see cref="DriveCommand"/>s for a
    /// <see cref="RoverController"/>. Up/down is throttle, left/right is turn.
    /// </summary>
    public sealed class ManualDriveInput : MonoBehaviour
    {
        [Tooltip("The rover to drive.")]
        [SerializeField] private RoverController controller;

        [Tooltip("A Vector2 action: y is throttle, x is turn. Player/Move in InputSystem_Actions works.")]
        [SerializeField] private InputActionReference moveAction;

        [Tooltip("Turn rate at full stick or key (deg/s).")]
        [SerializeField, Min(0f)] private float maxTurnRateDegrees = 90f;

        private void OnEnable()
        {
            if (controller == null || moveAction == null)
            {
                Debug.LogError($"{nameof(ManualDriveInput)} on '{name}' needs a controller and a move action.", this);
                enabled = false;
                return;
            }

            moveAction.action.Enable();
        }

        private void OnDisable()
        {
            if (controller != null)
                controller.Stop();
        }

        // Input is read every rendered frame; the controller applies it in FixedUpdate.
        private void Update()
        {
            Vector2 input = moveAction.action.ReadValue<Vector2>();

            float linear = input.y * controller.MaxWheelSpeed;
            // Pressing right (+x) should turn right, which is negative in our convention.
            float angular = -input.x * maxTurnRateDegrees * Mathf.Deg2Rad;

            controller.SetCommand(new DriveCommand(linear, angular));
        }
    }
}
