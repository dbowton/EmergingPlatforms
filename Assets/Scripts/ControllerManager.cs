using System.Collections.Generic;
using UnityEngine;

using InputDevice = UnityEngine.XR.InputDevice;
using VRButton = UnityEngine.XR.CommonUsages;

public class ControllerManager : MonoBehaviour
{
    [SerializeField] ControllerInitializer leftController;
    [SerializeField] ControllerInitializer rightController;

    public InputDevice leftInput;
    public InputDevice rightInput;

    [SerializeField] LineRenderer leftLineRenderer;
    [SerializeField] LineRenderer rightLineRenderer;

    public Vector3 LeftHandPos { get { return leftController.controllerObject.transform.position; } }
    public Vector3 RightHandPos { get { return rightController.controllerObject.transform.position; } }

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

    private List<GameObject> leftGameObjects = new List<GameObject>();
    private List<GameObject> rightGameObjects = new List<GameObject>();

    private void Update()
    {
        if(leftInput.GetControllerPressed(VRButton.trigger, out float pressedLeft) && pressedLeft > 0)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = Vector3.one * 0.25f;
            go.transform.position = LeftHandPos;

            Color color = Color.white;
            if (pressedLeft > 0.25) color = Color.yellow;
            if (pressedLeft > 0.5) color = Color.blue;
            if (pressedLeft > 0.75) color = Color.green;

            go.GetComponent<Renderer>().material.color = color;
            leftGameObjects.Add(go);
        }
        else if(leftInput.GetControllerPressed(VRButton.gripButton, out bool leftGripPressed) && leftGripPressed)
        {
            if(leftGameObjects.Count > 0)
            {
                GameObject temp = leftGameObjects[leftGameObjects.Count - 1];
                leftGameObjects.RemoveAt(leftGameObjects.Count - 1);
                Destroy(temp);
            }
        }
        else if (leftGameObjects.Count > 0 && leftInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 stick) && stick.sqrMagnitude > 0)
        {
            leftGameObjects[leftGameObjects.Count - 1].transform.position += new Vector3(stick.x, 0, stick.y) * Time.deltaTime;
        }


        if (rightInput.GetControllerPressed(VRButton.trigger, out float pressedRight) && pressedRight > 0)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = Vector3.one * 0.25f;
            go.transform.position = RightHandPos;

            Color color = Color.white;
            if (pressedRight > 0.25) color = Color.yellow;
            if (pressedRight > 0.5) color = Color.blue;
            if (pressedRight > 0.75) color = Color.green;

            go.GetComponent<Renderer>().material.color = color;


            rightGameObjects.Add(go);
        }
        else if (rightInput.GetControllerPressed(VRButton.gripButton, out bool rightGripPressed) && rightGripPressed)
        {
            if (rightGameObjects.Count > 0)
            {
                GameObject temp = rightGameObjects[rightGameObjects.Count - 1];
                rightGameObjects.RemoveAt(rightGameObjects.Count - 1);
                Destroy(temp);
            }
        }
        else if (rightGameObjects.Count > 0 && rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 stick) && stick.sqrMagnitude > 0)
        {
            rightGameObjects[rightGameObjects.Count - 1].transform.position += new Vector3(stick.x, 0, stick.y) * Time.deltaTime;
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
}
