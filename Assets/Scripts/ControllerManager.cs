using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using InputDevice = UnityEngine.XR.InputDevice;
using VRButton = UnityEngine.XR.CommonUsages;

public class ControllerManager : MonoBehaviour
{
    [SerializeField] ControllerInitializer leftController;
    [SerializeField] ControllerInitializer rightController;

    [SerializeField] InputDevice leftInput;
    [SerializeField] InputDevice rightInput;

    [SerializeField] LineRenderer leftLineRenderer;
    [SerializeField] LineRenderer rightLineRenderer;

    public Vector3 leftHandPos { get { return leftController.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.transform.position; } }
    public Vector3 rightHandPos { get { return rightController.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.transform.position; } }

    void Start() 
    {
        var leftHandedControllers = new List<InputDevice>();
        var desiredLeftCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredLeftCharacteristics, leftHandedControllers);

        var rightHandedControllers = new List<InputDevice>();
        var desiredRightCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredRightCharacteristics, rightHandedControllers);

        leftInput = leftHandedControllers[0];
        rightInput = rightHandedControllers[0];
    }

    private void Update()
    {
        if(leftInput.GetControllerHeld(VRButton.grip, out float test) && test > 0)
        {

        }


        DrawLine(rightLineRenderer, rightController, leftController);
        DrawLine(leftLineRenderer, leftController, rightController);
    }

    public void DrawLine(LineRenderer renderer, ControllerInitializer controller, ControllerInitializer other) {}

    public void OnMove(InputAction.CallbackContext context)
    {
        return;
//        movement = context.ReadValue<Vector2>();
    }

    public void Grab(ControllerInitializer active, ControllerInitializer other) {}
    public void Drop(ControllerInitializer active) {}
}

public static class ControllerExtensions
{
    public static bool GetControllerPressed(this InputDevice device, UnityEngine.XR.InputFeatureUsage<bool> button, out bool pressed)
    {
        return device.TryGetFeatureValue(button, out pressed);
    }

    public static bool GetControllerHeld(this InputDevice device, UnityEngine.XR.InputFeatureUsage<float> button, out float pressed)
    {
        return device.TryGetFeatureValue(button, out pressed);
    }
}
