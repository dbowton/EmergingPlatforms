using System.Collections.Generic;
using System.Linq;
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


    int currentGear = 0;
    float turnAmount = 0;
    float maxTurn = 10f;

    [SerializeField] GameObject car;
    [SerializeField] GameObject GearUI;
    bool inCar = false;

    private void Update()
    {
        switch (currentGear)
        {
            case -1:
                GearUI.GetComponent<Renderer>().material.color = Color.red;
                break;
            case 0:
                GearUI.GetComponent<Renderer>().material.color = Color.white;
                break;
            case 1:
                GearUI.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case 2:
                GearUI.GetComponent<Renderer>().material.color = Color.green;
                break;
            case 3:
                GearUI.GetComponent<Renderer>().material.color = Color.blue;
                break;
            case 4:
                GearUI.GetComponent<Renderer>().material.color = Color.cyan;
                break;
            case 5:
                GearUI.GetComponent<Renderer>().material.color = Color.grey;
                break;
            case 6:
                GearUI.GetComponent<Renderer>().material.color = Color.magenta;
                break;
            default:
                break;
        }

        if(rightInput.GetControllerPressed(VRButton.gripButton, out bool rightGrab))
        {
            if(rightGrab)
            {
                if (rightController.HeldObject == null)
                {
                    List<Collider> grabbableObjects = Physics.OverlapSphere(RightHandPos, 0.125f).ToList().Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && grabPoint.grabType.Equals(GrabPoint.GrabType.GearShift) && x.gameObject != leftController.HeldObject).ToList();

                    if(grabbableObjects.Count > 0)
                    {
                        grabbableObjects[0].gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                        rightController.HeldObject = grabbableObjects[0].gameObject;
                    }
                }
            }
            else if(rightController.HeldObject != null)
            {
                if (Vector3.Distance(RightHandPos, rightController.HeldObject.GetComponent<GrabPoint>().Extreme1.transform.position) < Vector3.Distance(RightHandPos, rightController.HeldObject.transform.position))
                {
                    currentGear = Mathf.Min(currentGear + 1, 9);
                    rightController.HeldObject.GetComponent<Renderer>().material.color = Color.green;
                }
                else if (Vector3.Distance(RightHandPos, rightController.HeldObject.GetComponent<GrabPoint>().Extreme2.transform.position) < Vector3.Distance(RightHandPos, rightController.HeldObject.transform.position))
                {
                    currentGear = Mathf.Max(currentGear - 1, -1);
                    rightController.HeldObject.GetComponent<Renderer>().material.color = Color.red;
                }

                rightController.HeldObject = null;
            }
        }

        if (leftInput.GetControllerPressed(VRButton.gripButton, out bool leftGrabbed))
        {
            if (leftGrabbed)
            {
                if (leftController.HeldObject == null)
                {
                    List<Collider> grabbableObjects = Physics.OverlapSphere(LeftHandPos, 0.125f).ToList().Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && grabPoint.grabType.Equals(GrabPoint.GrabType.Steering) && x.gameObject != rightController.HeldObject).ToList();

                    if (grabbableObjects.Count > 0)
                    {
                        grabbableObjects[0].gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                        leftController.HeldObject = grabbableObjects[0].gameObject;
                    }
                }
                else
                {

                    float leftDistance = Vector3.Distance(leftController.HeldObject.GetComponent<GrabPoint>().Extreme1.transform.position, LeftHandPos);
                    float rightDistance = Vector3.Distance(leftController.HeldObject.GetComponent<GrabPoint>().Extreme2.transform.position, LeftHandPos);

                    if(rightDistance > leftDistance)
                    {
                        leftController.HeldObject.GetComponent<GrabPoint>().Extreme1.GetComponent<Renderer>().material.color = Color.blue;
                        leftController.HeldObject.GetComponent<GrabPoint>().Extreme2.GetComponent<Renderer>().material.color = Color.red;
                    }
                    else
                    {
                        leftController.HeldObject.GetComponent<GrabPoint>().Extreme1.GetComponent<Renderer>().material.color = Color.red;
                        leftController.HeldObject.GetComponent<GrabPoint>().Extreme2.GetComponent<Renderer>().material.color = Color.blue;
                    }

                    float totalDistance = rightDistance - leftDistance;
                    turnAmount = -(maxTurn * totalDistance * Time.deltaTime);
                }
            }
            else
            {
                turnAmount = 0;
                leftController.HeldObject = null;
            }
        }

        if (!inCar && rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 dir) && dir.sqrMagnitude > 0)
        {            
            Vector3 adjusedForward = Camera.main.transform.forward;
            adjusedForward.y = 0;
            adjusedForward.Normalize();

            Vector3 adjusedRight = Camera.main.transform.right;
            adjusedRight.y = 0;
            adjusedRight.Normalize();

            transform.position += ((((adjusedForward) * (dir.y)) + ((dir.x) * (adjusedRight))) * Time.deltaTime);
        }
        if(!inCar && leftInput.GetControllerPressed(VRButton.gripButton, out bool leftGripped) && leftGripped)
        {
            inCar = true;
            transform.parent = car.transform;
        }

        if(inCar)
        {
            float speed = 5f * currentGear;
            transform.parent.position += Time.deltaTime * speed * transform.parent.forward;
            transform.parent.Rotate(transform.up, turnAmount * speed);
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
