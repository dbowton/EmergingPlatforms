using System;
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

        if(songs.Count > 0)
            radio.clip = songs[0];

        UpdateGearUI();
        UpdateRadioUI();
    }

    [SerializeField] GameObject steeringWheel;

    bool readyForNewGear = true;
    bool readyForNewRadio = true;

    int currentGear = 0;
    float turnAmount = 0;
    float maxTurn = 10f;

    float volumeChange = 0.2f;

    [SerializeField] GameObject car;
    
    [SerializeField] TMPro.TMP_Text GearUI;
    [SerializeField] TMPro.TMP_Text SpeedUI;

    bool inCar = false;
    [SerializeField] AudioSource radio;



    [SerializeField] List<AudioClip> songs = new List<AudioClip>();
    int songIndex = 0;

    private float grabRange = 0.0625f;

    [SerializeField] TMPro.TMP_Text clock;


    private void Update()
    {
        clock.text = DateTime.Now.Hour + ":" + DateTime.Now.Minute;

        if(inCar && rightInput.GetControllerPressed(VRButton.gripButton, out bool rightGrab))
        {
            if(rightGrab)
            {
                if (rightController.HeldObject == null)
                {
                    List<Collider> grabbableObjects = Physics.OverlapSphere(RightHandPos, grabRange).ToList().Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && (grabPoint.grabType.Equals(GrabPoint.GrabType.GearShift) || grabPoint.grabType.Equals(GrabPoint.GrabType.Radio)) && x.gameObject != leftController.HeldObject).ToList();

                    if(grabbableObjects.Count > 0)
                    {
                        grabbableObjects[0].gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                        rightController.HeldObject = grabbableObjects[0].gameObject;
                    }
                }
                else
                {
                    if(rightController.HeldObject.GetComponent<GrabPoint>().grabType.Equals(GrabPoint.GrabType.GearShift))
                    {
                        if(rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 rightAxis))
                        {
                        if (!readyForNewGear && rightAxis.sqrMagnitude == 0) readyForNewGear = true;
                        else if(readyForNewGear)
                        {
                            if(rightAxis.y > 0.9f)
                            {
                                //  upShift
                                int newGear = Mathf.Min(currentGear + 1, 7);
                                if(newGear != currentGear)
                                {
                                    currentGear = newGear;

                                    rightInput.SendHaptic(0.6f, 0.2f);
                                    leftInput.SendHaptic(0.6f, 0.2f);

                                    UpdateGearUI();
                                    rightController.HeldObject.GetComponent<Renderer>().material.color = Color.green;
                                    readyForNewGear = false;
                                }
                            }
                            else if(rightAxis.y < -0.9f)
                            {
                                //  downShift
                                int newGear = Mathf.Max(currentGear - 1, -1);
                                if (newGear != currentGear)
                                {
                                    currentGear = newGear;

                                    rightInput.SendHaptic(0.25f, 0.1f);
                                    leftInput.SendHaptic(0.25f, 0.1f);

                                    UpdateGearUI();
                                    rightController.HeldObject.GetComponent<Renderer>().material.color = Color.green;
                                    readyForNewGear = false;
                                }
                            }
                        }
                    }
                    }
                    else if(rightController.HeldObject.GetComponent<GrabPoint>().grabType.Equals(GrabPoint.GrabType.Radio))
                    {
                        if (rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 rightAxis))
                        {
                            if (!readyForNewRadio && rightAxis.sqrMagnitude == 0) readyForNewRadio = true;
                            else if(readyForNewRadio)
                            {
                                if(rightAxis.y > 0.9f)
                                {
                                    //  volume up
                                    radio.volume = Mathf.Min(1, radio.volume + volumeChange);
                                    readyForNewRadio = false;
                                    UpdateRadioUI();
                                }
                                else if(rightAxis.y < -0.9f)
                                {
                                    //  volume down
                                    radio.volume = Mathf.Max(0, radio.volume - volumeChange);
                                    readyForNewRadio = false;
                                    UpdateRadioUI();
                                }
                                else if(rightAxis.x > 0.9f)
                                {
                                    //  nextSong
                                    songIndex++;
                                    if (songIndex >= songs.Count) songIndex = 0;
                                    radio.Stop();
                                    radio.clip = songs[songIndex];
                                    radio.Play();
                                    readyForNewRadio = false;
                                    UpdateRadioUI();
                                }
                                else if(rightAxis.x < -0.9f)
                                {
                                    //  prevSong
                                    songIndex--;
                                    if (songIndex < 0) songIndex = songs.Count - 1;
                                    radio.Stop();
                                    radio.clip = songs[songIndex];
                                    radio.Play();
                                    readyForNewRadio = false;
                                    UpdateRadioUI();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if(rightController.HeldObject)
                {
                    rightController.HeldObject.GetComponent<Renderer>().material.color = Color.white;
                    rightController.HeldObject = null;
                }

                readyForNewGear = true;
            }
        }
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        if(inCar && leftInput.GetControllerPressed(VRButton.gripButton, out bool leftGrabbed))
        {
            if (leftGrabbed)
            {
                if (leftController.HeldObject == null)
                {
                    List<Collider> grabbableObjects = Physics.OverlapSphere(LeftHandPos, grabRange).ToList().Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && grabPoint.grabType.Equals(GrabPoint.GrabType.Steering) && x.gameObject != rightController.HeldObject).ToList();

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

                    float totalDistance = rightDistance - leftDistance;
                    turnAmount = -(maxTurn * totalDistance * Time.deltaTime);
                }
            }
            else
            {
                turnAmount = 0;
                if (leftController.HeldObject)
                {
                    leftController.HeldObject.GetComponent<Renderer>().material.color = Color.white;
                    leftController.HeldObject = null;
                }
            }
        }

        if(!inCar && rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 dir) && dir.sqrMagnitude > 0)
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
            Vector3 rot = steeringWheel.transform.localEulerAngles;
            if (rot.z > 180) rot.z -= 360;
            rot.z = Mathf.Lerp(rot.z, -turnAmount * 90 * 18, 0.8f);
            steeringWheel.transform.localEulerAngles = rot;
            float speed = 5f * currentGear;
            transform.parent.position += Time.deltaTime * speed * transform.parent.forward;
            transform.parent.Rotate(transform.up, turnAmount * speed);
        }
    }

    private void UpdateGearUI()
    {
        string gearText = currentGear.ToString();
        Color textColor = Color.white;

        if(currentGear == 0)
        {
            gearText = "N";
            textColor = Color.yellow;
        }
        else if(currentGear == -1)
        {
            gearText = "R";
            textColor = Color.red;
        }

        GearUI.text = gearText;
        GearUI.color = textColor;
    }

    [SerializeField] TMPro.TMP_Text radioName;
    [SerializeField] TMPro.TMP_Text volumeLevel;

    private void UpdateRadioUI()
    {
        radioName.text = songs[songIndex].name;

        string volumeText = (radio.volume * 100) + "%";

        Color volumeColor;
        if (radio.volume == 0) volumeColor = Color.black;
        else
        {
            Color minColor = Color.red;
            Color maxColor = Color.white;

            minColor *= 1 - radio.volume;
            maxColor *= radio.volume;

            volumeColor = minColor + maxColor;
        }

        radioName.color = (radio.volume == 0) ? Color.black : Color.white;

        volumeLevel.text = volumeText;
        volumeLevel.color = volumeColor;
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
