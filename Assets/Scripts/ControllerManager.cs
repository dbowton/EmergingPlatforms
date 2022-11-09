using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using InputDevice = UnityEngine.XR.InputDevice;
using VRButton = UnityEngine.XR.CommonUsages;

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

    bool canChangeCar = true;

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

    Vehicle car = null;

    private static float playerGrabRange = 0.0625f;


    private void Update()
    {
        if(car == null)
        {
            if (rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 dir) && dir.sqrMagnitude > 0)
            {
                Vector3 adjusedForward = Camera.main.transform.forward;
                adjusedForward.y = 0;
                adjusedForward.Normalize();

                Vector3 adjusedRight = Camera.main.transform.right;
                adjusedRight.y = 0;
                adjusedRight.Normalize();

                transform.position += ((((adjusedForward) * (dir.y)) + ((dir.x) * (adjusedRight))) * Time.deltaTime);
            }
        }
        else
            car.UpdateVehicle();


        if (leftInput.GetControllerPressed(VRButton.gripButton, out bool leftGripped))
        {
            if (!canChangeCar && !leftGripped) 
                canChangeCar = true;
            else if(canChangeCar && leftGripped)
            {
                List<Collider> grabbableObjects = Physics.OverlapSphere(ControllerManager.LeftHandPos, playerGrabRange).ToList().Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && grabPoint.grabType.Equals(GrabPoint.GrabType.VehicleEntry)).ToList();

                if(grabbableObjects.Count > 0)
                {
                    if (car)
                    {
                        if (car.targetGear == 0)
                        {
                            //  leave vehicle
                            car.Leave();
                            car = null;
                            transform.parent = null;
                            canChangeCar = false;
                        }
                    }
                    else
                    {
                        foreach (var grab in grabbableObjects)
                        {
                            if (grab.gameObject.transform.parent.TryGetComponent<Vehicle>(out Vehicle newVehicle))
                            {
                                car = newVehicle;
                                transform.parent = car.transform;
                                canChangeCar = false;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
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
