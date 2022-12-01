using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using UnityEngine;

using VRButton = UnityEngine.XR.CommonUsages;

public class Vehicle : MonoBehaviour
{
    public Transform seatPosition;
    [SerializeField] TMPro.TMP_Text clock;
    [SerializeField] TMPro.TMP_Text radioName;
    [SerializeField] TMPro.TMP_Text volumeLevel;

    [SerializeField] TMPro.TMP_Text GearUI;
    [SerializeField] TMPro.TMP_Text SpeedUI;

    [SerializeField] GameObject steeringWheel;
    [SerializeField] Rigidbody controller;

    bool readyForNewGear = true;
    bool readyForNewRadio = true;

    public bool IsStopped { get { return currentGear == 0; } }
    [HideInInspector] public bool isOn = false;

    public float baseSpeed = 5f;
    public float turnMulti = 18f;

    float speed = 0;


    private int targetGear = 0;
    private float currentGear = 0;
    private float gearPenaltyMulti = 1;

    float turnAmount = 0;
    readonly float maxTurn = 10f;
    readonly float volumeChange = 0.2f;
    readonly float vehicleGrabRange = 0.0625f;

    public List<AudioSource> radioSpeakers;

    int songIndex = 0;

    bool engineSoundsQueued = false;

    [SerializeField] AudioSource engineSound;
    [SerializeField] AudioClip engineStartUpSound;
    [SerializeField] AudioClip engineIdleSound;
    [SerializeField] AudioClip engineShutdownSound;

    [SerializeField] List<WheelCollider> steeringWheels;
    [SerializeField] List<WheelCollider> drivingWheels;

    private void Start()
    {
        if (RadioStation.instance.songs.Count > 0)
        {
            foreach (var speaker in radioSpeakers)
                speaker.clip = RadioStation.instance.songs[UnityEngine.Random.Range(0, RadioStation.instance.songs.Count)];
        }

        controller.centerOfMass -= Vector3.up;

        UpdateGearUI();
        UpdateRadioUI();
    }

    private void Update()
    {
        if(clock)
            clock.text = DateTime.Now.Hour + ":" + DateTime.Now.Minute;

        if(engineIdleSound && engineSoundsQueued && !engineSound.isPlaying)
        {
            engineSoundsQueued = false;
            engineSound.clip = engineIdleSound;

            engineSound.loop = true;
            engineSound.volume = 0.25f;
            engineSound.Play();
        }

        foreach (var wheel in drivingWheels)
        {
            wheel.motorTorque = Mathf.Abs(targetGear) * speed;
            wheel.brakeTorque = (targetGear == 0) ? ((ebraking) ? 1000f : 250f) : 0f;
        }

        foreach (var wheel in steeringWheels)
            wheel.steerAngle = turnAmount * 90f;
    }

    bool ebraking = false;

    public void UpdateVehicle(Player player, float dt)
    {
        ebraking = false;
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

                                        ControllerManager.rightController.HeldObject.GetComponent<Renderer>().material.color = Color.red;
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
                                    if (songIndex >= RadioStation.instance.songs.Count) songIndex = 0;

                                    foreach (var speaker in radioSpeakers)
                                    {
                                        speaker.Stop();
                                        speaker.clip = RadioStation.instance.songs[songIndex];
                                        speaker.Play();
                                    }
                                    readyForNewRadio = false;
                                    UpdateRadioUI();
                                }
                                else if (rightAxis.x < -0.9f)
                                {
                                    //  prevSong
                                    songIndex--;
                                    if (songIndex < 0) songIndex = RadioStation.instance.songs.Count - 1;

                                    foreach (var speaker in radioSpeakers)
                                    {
                                        speaker.Stop();
                                        speaker.clip = RadioStation.instance.songs[songIndex];
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
                        if (targetGear != 0) ControllerManager.rightInput.SendHaptic(0.1f * Mathf.Abs(targetGear), 0.2f * Mathf.Abs(targetGear));
                        if (targetGear != 0) ControllerManager.leftInput.SendHaptic(0.1f * Mathf.Abs(targetGear), 0.2f * Mathf.Abs(targetGear));

                        targetGear = 0;
                        ebraking = true;
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
        
        UpdateGearUI(dt);

        Vector3 rot = steeringWheel.transform.localEulerAngles;

        if(Mathf.Abs(steeringRotAxii.x) == 1)
        {
            if (rot.x > 180) rot.x -= 360;
            rot.x = Mathf.Lerp(rot.x, -turnAmount * 90 * turnMulti * steeringRotAxii.x, 0.8f);
        }
        else if (Mathf.Abs(steeringRotAxii.y) == 1)
        {
            if (rot.y > 180) rot.y -= 360;
            rot.y = Mathf.Lerp(rot.y, -turnAmount * 90 * turnMulti * steeringRotAxii.y, 0.8f);
        }
        else if (Mathf.Abs(steeringRotAxii.z * steeringRotAxii.z) == 1)
        {
            if (rot.z > 180) rot.z -= 360;
            rot.z = Mathf.Lerp(rot.z, -turnAmount * 90 * turnMulti, 0.8f);
        }

        steeringWheel.transform.localEulerAngles = rot;
        speed = baseSpeed * currentGear * gearPenaltyMulti * ((currentGear < 0) ? 2 : 1);
    }

    [SerializeField] Vector3 steeringRotAxii;

    public void TurnOff()
    {
        isOn = false;

        foreach(var speaker in radioSpeakers)
        {
            speaker.volume = 0;
        }

        UpdateRadioUI();
        UpdateGearUI();

        if(clock) clock.color = Color.black;
        if(SpeedUI) SpeedUI.color = Color.black;
        if(GearUI) GearUI.color = Color.black;

        engineSound.Stop();
        engineSoundsQueued = false;

        if (engineShutdownSound)
        {
            engineSound.clip = engineShutdownSound;
            engineSound.loop = false;
            engineSound.volume = 0.5f;
            engineSound.Play();
        }
    }

    public void TurnOn()
    {
        isOn = true;

        if(clock) clock.color = Color.white;
        if(SpeedUI) SpeedUI.color = Color.white;
        UpdateGearUI();
        UpdateRadioUI();

        engineSound.Play();

        if(engineStartUpSound)
        {
            engineSound.clip = engineStartUpSound;
            engineSound.loop = false;
            engineSound.volume = 0.5f;
            engineSound.Play();
        }
        engineSoundsQueued = true;
    }

    private void UpdateGearUI(float dt = 1)
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

        if (!isOn) gearText = "";

        if(GearUI) GearUI.text = gearText;
        if (GearUI) GearUI.color = textColor;

        if (SpeedUI)
        {
            clockedSpeed.Add((transform.position, dt));
            if (clockedSpeed.Count > 5) clockedSpeed.RemoveAt(0);

            float newSpeed = Vector3.Distance(clockedSpeed[0].pos, clockedSpeed[clockedSpeed.Count - 1].pos) / clockedSpeed.Sum(x => x.time);
            newSpeed = Mathf.Lerp(prevSpeed, newSpeed, 0.3f);
            prevSpeed = newSpeed;
            SpeedUI.text = (newSpeed * 1.5f).ToString("0.0#");
        }
    }

    float prevSpeed = 0;
    List<(Vector3 pos, float time)> clockedSpeed = new List<(Vector3, float)>();


    private void UpdateRadioUI()
    {
        if(radioName) radioName.text = RadioStation.instance.songs[songIndex].name;

        if (radioSpeakers.Count > 0)
        {
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

            if(radioName) radioName.color = (radioSpeakers[0].volume == 0) ? Color.black : Color.white;

            if(volumeLevel) volumeLevel.text = volumeText;
            if(volumeLevel) volumeLevel.color = volumeColor;
        }
    }
}
