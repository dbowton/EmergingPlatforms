using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using VRButton = UnityEngine.XR.CommonUsages;

public class Vehicle : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text clock;
    [SerializeField] TMPro.TMP_Text radioName;
    [SerializeField] TMPro.TMP_Text volumeLevel;

    [SerializeField] TMPro.TMP_Text GearUI;
    [SerializeField] TMPro.TMP_Text SpeedUI;

    [SerializeField] GameObject steeringWheel;

    bool readyForNewGear = true;
    bool readyForNewRadio = true;

    public bool IsStopped { get { return currentGear == 0; } }
    [HideInInspector] public bool isOn = false;

    private int targetGear = 0;
    private float currentGear = 0;
    private float gearPenaltyMulti = 1;
    private float breakMulti = 1;

    float turnAmount = 0;
    readonly float maxTurn = 10f;
    readonly float volumeChange = 0.2f;
    readonly float vehicleGrabRange = 0.0625f;

    public List<AudioSource> radioSpeakers;

    [SerializeField] List<AudioClip> songs = new List<AudioClip>();
    int songIndex = 0;

    bool engineSoundsQueued = false;

    [SerializeField] AudioSource engineSound;
    [SerializeField] AudioClip engineStartUpSound;
    [SerializeField] AudioClip engineIdleSound;
    [SerializeField] AudioClip engineShutdownSound;

    private void Start()
    {
        if (songs.Count > 0)
        {
            foreach (var speaker in radioSpeakers)
                speaker.clip = songs[0];
        }

        UpdateGearUI();
        UpdateRadioUI();
    }

    private void Update()
    {
        clock.text = DateTime.Now.Hour + ":" + DateTime.Now.Minute;

        if(engineSoundsQueued && !engineSound.isPlaying)
        {
            engineSoundsQueued = false;
            engineSound.clip = engineIdleSound;

            engineSound.loop = true;
            engineSound.volume = 0.25f;
            engineSound.Play();
        }
    }

    public void UpdateVehicle()
    {
        breakMulti = 1;
        if (ControllerManager.rightInput.GetControllerPressed(VRButton.gripButton, out bool rightGrab))
        {
            if (rightGrab)
            {
                if (ControllerManager.rightController.HeldObject == null)
                {
                    List<Collider> grabbableObjects = Physics.OverlapSphere(ControllerManager.RightHandPos, vehicleGrabRange).ToList().Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && (grabPoint.grabType.Equals(GrabPoint.GrabType.GearShift) || grabPoint.grabType.Equals(GrabPoint.GrabType.Radio) || grabPoint.grabType.Equals(GrabPoint.GrabType.EBrake)) && x.gameObject != ControllerManager.leftController.HeldObject).ToList();

                    if (grabbableObjects.Count > 0)
                    {
                        grabbableObjects[0].gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                        ControllerManager.rightController.HeldObject = grabbableObjects[0].gameObject;
                    }
                }
                else
                {
                    if (ControllerManager.rightController.HeldObject.GetComponent<GrabPoint>().grabType.Equals(GrabPoint.GrabType.GearShift))
                    {
                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 rightAxis))
                        {
                            if (!readyForNewGear && rightAxis.sqrMagnitude == 0) readyForNewGear = true;
                            else if (readyForNewGear)
                            {
                                if (rightAxis.y > 0.9f)
                                {
                                    //  upShift
                                    int newGear = Mathf.Min(targetGear + 1, 7);
                                    if (newGear != targetGear)
                                    {
                                        targetGear = newGear;

                                        float gearDif = Mathf.Abs(currentGear - targetGear);

                                        if(gearDif <= 1)
                                        {
                                            ControllerManager.rightInput.SendHaptic(0.6f, 0.2f);
                                            ControllerManager.leftInput.SendHaptic(0.6f, 0.2f);
                                        }
                                        else if (gearDif <= 1.5f)
                                        {
                                            ControllerManager.rightInput.SendHaptic(0.8f, 0.3f);
                                            ControllerManager.leftInput.SendHaptic(0.8f, 0.3f);
                                        }
                                        else
                                        {
                                            ControllerManager.rightInput.SendHaptic(1f, 0.5f);
                                            ControllerManager.leftInput.SendHaptic(1f, 0.5f);
                                        }

                                        ControllerManager.rightController.HeldObject.GetComponent<Renderer>().material.color = Color.green;
                                        readyForNewGear = false;
                                    }
                                }
                                else if (rightAxis.y < -0.9f)
                                {
                                    //  downShift
                                    int newGear = Mathf.Max(targetGear - 1, -1);
                                    if (newGear != targetGear)
                                    {
                                        targetGear = newGear;

                                        float gearDif = Mathf.Abs(currentGear - targetGear);

                                        if (gearDif <= 1)
                                        {
                                            ControllerManager.rightInput.SendHaptic(0.25f, 0.1f);
                                            ControllerManager.leftInput.SendHaptic(0.25f, 0.1f);
                                        }
                                        else if (gearDif <= 1.5f)
                                        {
                                            ControllerManager.rightInput.SendHaptic(0.4f, 0.2f);
                                            ControllerManager.leftInput.SendHaptic(0.4f, 0.2f);
                                        }
                                        else
                                        {
                                            ControllerManager.rightInput.SendHaptic(0.65f, 0.4f);
                                            ControllerManager.leftInput.SendHaptic(0.65f, 0.4f);
                                        }

                                        ControllerManager.rightController.HeldObject.GetComponent<Renderer>().material.color = Color.green;
                                        readyForNewGear = false;
                                    }
                                }
                            }
                        }
                    }
                    else if (ControllerManager.rightController.HeldObject.GetComponent<GrabPoint>().grabType.Equals(GrabPoint.GrabType.Radio))
                    {
                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 rightAxis))
                        {
                            if (!readyForNewRadio && rightAxis.sqrMagnitude == 0) readyForNewRadio = true;
                            else if (readyForNewRadio)
                            {
                                if (rightAxis.y > 0.9f)
                                {
                                    //  volume up
                                    foreach (var speaker in radioSpeakers)
                                    {
                                        speaker.volume = Mathf.Min(1, (Mathf.Round(speaker.volume * 10) / 10f) + volumeChange);
                                        if (!speaker.isPlaying) speaker.Play();
                                    }
                                    readyForNewRadio = false;
                                    UpdateRadioUI();
                                }
                                else if (rightAxis.y < -0.9f)
                                {
                                    //  volume down
                                    foreach (var speaker in radioSpeakers)
                                    {
                                        speaker.volume = Mathf.Max(0, (Mathf.Round(speaker.volume * 10) / 10f) - volumeChange);
                                        if (!speaker.isPlaying) speaker.Play();
                                    }
                                    readyForNewRadio = false;
                                    UpdateRadioUI();
                                }
                                else if (rightAxis.x > 0.9f)
                                {
                                    //  nextSong
                                    songIndex++;
                                    if (songIndex >= songs.Count) songIndex = 0;

                                    foreach (var speaker in radioSpeakers)
                                    {
                                        speaker.Stop();
                                        speaker.clip = songs[songIndex];
                                        speaker.Play();
                                    }
                                    readyForNewRadio = false;
                                    UpdateRadioUI();
                                }
                                else if (rightAxis.x < -0.9f)
                                {
                                    //  prevSong
                                    songIndex--;
                                    if (songIndex < 0) songIndex = songs.Count - 1;

                                    foreach (var speaker in radioSpeakers)
                                    {
                                        speaker.Stop();
                                        speaker.clip = songs[songIndex];
                                        speaker.Play();
                                    }

                                    readyForNewRadio = false;
                                    UpdateRadioUI();
                                }
                            }
                        }
                    }
                    else if (ControllerManager.rightController.HeldObject.GetComponent<GrabPoint>().grabType.Equals(GrabPoint.GrabType.EBrake))
                    {
                        breakMulti = 0;
                    }
                }
            }
            else
            {
                if (ControllerManager.rightController.HeldObject)
                {
                    ControllerManager.rightController.HeldObject.GetComponent<Renderer>().material.color = Color.white;
                    ControllerManager.rightController.HeldObject = null;
                }

                readyForNewGear = true;
            }
        }
        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
        if (ControllerManager.leftInput.GetControllerPressed(VRButton.gripButton, out bool leftGrabbed))
        {
            if (leftGrabbed)
            {
                if (ControllerManager.leftController.HeldObject == null)
                {
                    List<Collider> grabbableObjects = Physics.OverlapSphere(ControllerManager.LeftHandPos, vehicleGrabRange).ToList().Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && grabPoint.grabType.Equals(GrabPoint.GrabType.Steering) && x.gameObject != ControllerManager.rightController.HeldObject).ToList();

                    if (grabbableObjects.Count > 0)
                    {
                        grabbableObjects[0].gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                        ControllerManager.leftController.HeldObject = grabbableObjects[0].gameObject;
                    }
                }
                else
                {

                    float leftDistance = Vector3.Distance(ControllerManager.leftController.HeldObject.GetComponent<GrabPoint>().Extreme1.transform.position, ControllerManager.LeftHandPos);
                    float rightDistance = Vector3.Distance(ControllerManager.leftController.HeldObject.GetComponent<GrabPoint>().Extreme2.transform.position, ControllerManager.LeftHandPos);

                    float totalDistance = rightDistance - leftDistance;
                    turnAmount = -(maxTurn * totalDistance * Time.deltaTime);
                }
            }
            else
            {
                turnAmount = 0;
                if (ControllerManager.leftController.HeldObject)
                {
                    ControllerManager.leftController.HeldObject.GetComponent<Renderer>().material.color = Color.white;
                    ControllerManager.leftController.HeldObject = null;
                }
            }
        }

        if (Mathf.Abs(currentGear - targetGear) < 0.05f)
            currentGear = targetGear;
        else
            currentGear = Mathf.Lerp(currentGear, targetGear, 0.02f);
        
        UpdateGearUI();

        Vector3 rot = steeringWheel.transform.localEulerAngles;
        if (rot.z > 180) rot.z -= 360;
        rot.z = Mathf.Lerp(rot.z, -turnAmount * 90 * 18, 0.8f);
        steeringWheel.transform.localEulerAngles = rot;
        float speed = 5f * currentGear * gearPenaltyMulti * breakMulti;
        transform.position += Time.deltaTime * speed * transform.forward;
        transform.Rotate(transform.up, turnAmount * speed);
    }


    public void TurnOff()
    {
        isOn = false;

        foreach(var speaker in radioSpeakers)
        {
            speaker.volume = 0;
        }

        UpdateRadioUI();
        UpdateGearUI();

        clock.color = Color.black;
        SpeedUI.color = Color.black;
        GearUI.color = Color.black;

        engineSound.Stop();
        engineSoundsQueued = false;

        engineSound.clip = engineShutdownSound;
        engineSound.loop = false;
        engineSound.volume = 0.5f;
        engineSound.Play();
    }

    public void TurnOn()
    {
        isOn = true;

        clock.color = Color.white;
        SpeedUI.color = Color.white;
        UpdateGearUI();
        UpdateRadioUI();

        engineSound.Play();

        engineSound.clip = engineStartUpSound;
        engineSound.loop = false;
        engineSound.volume = 0.5f;
        engineSound.Play();
        engineSoundsQueued = true;
    }

    private void UpdateGearUI()
    {
        string gearText = targetGear.ToString();
        Color textColor = Color.white;

        if (targetGear == 0)
        {
            gearText = "N";
            textColor = Color.yellow;
        }
        else if (targetGear == -1)
        {
            gearText = "R";
            textColor = Color.red;
        }

        GearUI.text = gearText;
        GearUI.color = textColor;

        SpeedUI.text = (Mathf.Round(Time.deltaTime * 5f * Mathf.Abs(currentGear) * gearPenaltyMulti * breakMulti * 60 * 60 * 10) / 100f).ToString();
    }
    private void UpdateRadioUI()
    {
        radioName.text = songs[songIndex].name;

        string volumeText = (radioSpeakers[0].volume * 100) + "%";

        Color volumeColor;
        if (radioSpeakers[0].volume == 0) volumeColor = Color.black;
        else
        {
            Color minColor = Color.red;
            Color maxColor = Color.white;

            minColor *= 1 - radioSpeakers[0].volume;
            maxColor *= radioSpeakers[0].volume;

            volumeColor = minColor + maxColor;
        }

        radioName.color = (radioSpeakers[0].volume == 0) ? Color.black : Color.white;

        volumeLevel.text = volumeText;
        volumeLevel.color = volumeColor;
    }
}
