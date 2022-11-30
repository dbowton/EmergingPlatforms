using System;
using System.Collections.Generic;
using UnityEngine;

using InputDevice = UnityEngine.XR.InputDevice;

public class ControllerManager : MonoBehaviour
{
    [SerializeField] ControllerInitializer initLeftController;
    [SerializeField] ControllerInitializer initRightController;

    public static ControllerInitializer leftController;
    public static ControllerInitializer rightController;

    public static InputDevice leftInput;
    public static InputDevice rightInput;

    public static Vector3 LeftHandPos { get { return leftController.controllerObject.transform.position; } }
    public static Vector3 RightHandPos { get { return rightController.controllerObject.transform.position; } }

    void Start() 
    {
        leftController = initLeftController;
        rightController = initRightController;

        var leftHandedControllers = new List<InputDevice>();
        var desiredLeftCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredLeftCharacteristics, leftHandedControllers);

        var rightHandedControllers = new List<InputDevice>();
        var desiredRightCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredRightCharacteristics, rightHandedControllers);

        leftInput = leftHandedControllers[0];
        rightInput = rightHandedControllers[0];
    }

    public static float playerGrabRange = 0.0625f;
}




public static class ControllerExtensions
{
    public static bool GetControllerPressed(this InputDevice device, UnityEngine.XR.InputFeatureUsage<bool> button, out bool pressed)
    {
        return device.TryGetFeatureValue(button, out pressed);
    }

    public static bool GetControllerPressed(this InputDevice device, UnityEngine.XR.InputFeatureUsage<float> button, out float pressed)
    {
        return device.TryGetFeatureValue(button, out pressed);
    }

    public static bool GetControllerPressed(this InputDevice device, UnityEngine.XR.InputFeatureUsage<Vector2> button, out Vector2 pressed)
    {
        return device.TryGetFeatureValue(button, out pressed);
    }

    public static bool SendHaptic(this InputDevice device, float intensity, float duration = 1f)
    {
        return device.SendHapticImpulse(0, intensity, duration);
    }
}
