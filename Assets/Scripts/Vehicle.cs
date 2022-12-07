using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using InputDevice = UnityEngine.XR.InputDevice;
using VRButton = UnityEngine.XR.CommonUsages;

public class Vehicle : MonoBehaviour
{
    [SerializeField] bool ToggleDoor = false;

    private void OnValidate()
    {
        if (ToggleDoor)
        {
            OperateDoor();
            ToggleDoor = false;
        }
    }

    [SerializeField] VehicleColorChanger vehicleColor;

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

    float speed = 0;

    private int targetGear = 0;
    private float currentGear = 0;

    float turning = 0;

    readonly float maxTurn = 120f;
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

    Vector3 defaultSteeringRotation;
    bool ebraking = false;
    [SerializeField] Transform steeringWheelCenter;
    [SerializeField] Vector3 steeringRotAxii;

    [SerializeField] float turnMulti = 1 / 4f;

    float prevSpeed = 0;
    List<(Vector3 pos, float time)> clockedSpeed = new List<(Vector3, float)>();

    private void Start()
    {
        if (clock) clock.enabled = false;
        if (radioName) radioName.enabled = false;
        if (volumeLevel) volumeLevel.enabled = false;
        if (SpeedUI) SpeedUI.enabled = false;
        if (GearUI) GearUI.enabled = false;

        if (RadioStation.instance != null && RadioStation.instance.songs.Count > 0)
        {
            songIndex = UnityEngine.Random.Range(0, RadioStation.instance.songs.Count);
            foreach (var speaker in radioSpeakers)
                speaker.clip = RadioStation.instance.songs[songIndex];
        }

        controller.centerOfMass -= Vector3.up;
        defaultSteeringRotation = steeringWheel.transform.localEulerAngles;

        UpdateGearUI();
        UpdateRadioUI();
    }

    private void Update()
    {
        if(engineIdleSound && engineSoundsQueued && !engineSound.isPlaying)
        {
            engineSoundsQueued = false;
            engineSound.clip = engineIdleSound;

            engineSound.loop = true;
            engineSound.volume = 0.25f;
            engineSound.Play();
        }
    }

    private void FixedUpdate()
    {
        foreach (var wheel in drivingWheels)
        {
            wheel.motorTorque = Mathf.Abs(targetGear) * speed;
            wheel.brakeTorque = (targetGear == 0) ? ((ebraking) ? 1000f : 250f) : 0f;
        }

        foreach (var wheel in steeringWheels)
            wheel.steerAngle = turnMulti * ((turning > 180) ? turning - 360 : turning);
    }

    public void UpdateVehicle(float dt)
    {
        if (clock)
            clock.text = DateTime.Now.Hour + ":" + DateTime.Now.Minute;

        UpdateController(ControllerManager.Left, ControllerManager.Right);
        UpdateController(ControllerManager.Right, ControllerManager.Left);

        if (Mathf.Abs(currentGear - targetGear) < 0.05f)
            currentGear = targetGear;
        else if(currentGear * targetGear <= 0)
        {
            currentGear = Mathf.Lerp(currentGear, targetGear, 0.2f);
        }
        else currentGear = Mathf.Lerp(currentGear, targetGear, 0.02f);
        
        UpdateGearUI(dt);
        speed = baseSpeed * currentGear * ((currentGear < 0) ? 4 : 1);
    }

    [SerializeField] GameObject carDoor;
    [SerializeField] float doorOpenAmount;
    bool doorIsOpen = false;

    public void OperateDoor()
    {
        carDoor.transform.localRotation *= Quaternion.Euler((!doorIsOpen ? doorOpenAmount : -doorOpenAmount) * Vector3.up);
        doorIsOpen = !doorIsOpen;
    }

    private void UpdateController((InputDevice input, ControllerInitializer controller, Vector3 position) current, (InputDevice input, ControllerInitializer controller, Vector3 position) other)
    {
        if (current.input.GetControllerPressed(VRButton.gripButton, out bool grabbed))
        {
            if(grabbed)
            {
                if (current.controller.HeldObject == null)
                {
                    List<Collider> grabbableObjects = Physics.OverlapSphere(current.position, vehicleGrabRange)
                        .Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && 
                            (other.controller.HeldObject == null || !grabPoint.grabType.Equals(other.controller.HeldObject.GetComponent<GrabPoint>().grabType)))
                        .OrderBy(x => Vector3.Distance(current.position, x.transform.position)).ToList();

                    if (grabbableObjects.Count > 0)
                    {
                        grabbableObjects[0].gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                        current.controller.HeldObject = grabbableObjects[0].gameObject;
                    }
                }
                else
                {
                    //  Grab
                    switch (current.controller.HeldObject.GetComponent<GrabPoint>().grabType)
                    {
                        case GrabPoint.GrabType.Steering:
                            {
                                Vector3 baseVector = current.controller.HeldObject.transform.position - steeringWheelCenter.position;
                                Vector3 angledVector = current.position - steeringWheelCenter.position;

                                angledVector = steeringWheelCenter.position + Vector3.ProjectOnPlane(angledVector, steeringWheelCenter.up);
                                turning = Vector3.SignedAngle(baseVector, angledVector - steeringWheelCenter.position, steeringWheelCenter.up);

                                if (turning < 0) turning += 360;
                                if (turning > maxTurn && turning < 360 - maxTurn)
                                {
                                    if (MathF.Abs(turning - maxTurn) < Mathf.Abs(turning - (360 - maxTurn)))
                                    {
                                        turning = maxTurn;
                                    }
                                    else turning = 360 - maxTurn;
                                }

                                Vector3 rot = steeringWheel.transform.localEulerAngles;

                                if(steeringRotAxii.x == 1)
                                    steeringWheel.transform.localRotation = Quaternion.Euler(turning, rot.y, rot.z);
                                if (steeringRotAxii.y == 1)
                                    steeringWheel.transform.localRotation = Quaternion.Euler(rot.x, turning, rot.z);
                                if (steeringRotAxii.z == 1)
                                    steeringWheel.transform.localRotation = Quaternion.Euler(rot.x, rot.y, turning);

                            }
                            break;
                        case GrabPoint.GrabType.GearShift:
                            {
                                if (current.input.GetControllerPressed(VRButton.primary2DAxis, out Vector2 gearAxis))
                                {
                                    if (!readyForNewGear && gearAxis.sqrMagnitude == 0) readyForNewGear = true;
                                    else if (readyForNewGear)
                                    {
                                        if (gearAxis.y > 0.9f)
                                        {
                                            //  upShift
                                            int newGear = Mathf.Min(targetGear + 1, 7);
                                            if (newGear != targetGear)
                                            {
                                                targetGear = newGear;

                                                float gearDif = Mathf.Abs(currentGear - targetGear);

                                                if (gearDif <= 1)
                                                {
                                                    current.input.SendHaptic(0.6f, 0.2f);
                                                    other.input.SendHaptic(0.6f, 0.2f);
                                                }
                                                else if (gearDif <= 1.5f)
                                                {
                                                    current.input.SendHaptic(0.8f, 0.3f);
                                                    other.input.SendHaptic(0.8f, 0.3f);
                                                }
                                                else
                                                {
                                                    current.input.SendHaptic(1f, 0.5f);
                                                    other.input.SendHaptic(1f, 0.5f);
                                                }

                                                current.controller.HeldObject.GetComponent<Renderer>().material.color = Color.green;
                                                readyForNewGear = false;
                                            }
                                        }
                                        else if (gearAxis.y < -0.9f)
                                        {
                                            //  downShift
                                            int newGear = Mathf.Max(targetGear - 1, -1);
                                            if (newGear != targetGear)
                                            {
                                                targetGear = newGear;

                                                float gearDif = Mathf.Abs(currentGear - targetGear);

                                                if (gearDif <= 1)
                                                {
                                                    current.input.SendHaptic(0.25f, 0.1f);
                                                    other.input.SendHaptic(0.25f, 0.1f);
                                                }
                                                else if (gearDif <= 1.5f)
                                                {
                                                    current.input.SendHaptic(0.4f, 0.2f);
                                                    other.input.SendHaptic(0.4f, 0.2f);
                                                }
                                                else
                                                {
                                                    current.input.SendHaptic(0.65f, 0.4f);
                                                    other.input.SendHaptic(0.65f, 0.4f);
                                                }

                                                current.controller.HeldObject.GetComponent<Renderer>().material.color = Color.red;
                                                readyForNewGear = false;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case GrabPoint.GrabType.Radio:
                            {
                                if (current.input.GetControllerPressed(VRButton.primary2DAxis, out Vector2 radioAxis))
                                {
                                    if (!readyForNewRadio && radioAxis.sqrMagnitude == 0) readyForNewRadio = true;
                                    else if (readyForNewRadio)
                                    {
                                        if (radioAxis.y > 0.9f)
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
                                        else if (radioAxis.y < -0.9f)
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
                                        else if (radioAxis.x > 0.9f)
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
                                        else if (radioAxis.x < -0.9f)
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
                            break;
                        case GrabPoint.GrabType.VehicleEntry:
                            break;
                        case GrabPoint.GrabType.EBrake:
                            {
                                if (targetGear != 0) current.input.SendHaptic(0.1f * Mathf.Abs(targetGear), 0.2f * Mathf.Abs(targetGear));
                                if (targetGear != 0) other.input.SendHaptic(0.1f * Mathf.Abs(targetGear), 0.2f * Mathf.Abs(targetGear));

                                targetGear = 0;
                                ebraking = true;
                            }
                            break;
                        case GrabPoint.GrabType.VehicleStart:
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                //  release
                if (current.controller.HeldObject != null)
                {
                    switch (current.controller.HeldObject.GetComponent<GrabPoint>().grabType)
                    {
                        case GrabPoint.GrabType.Steering:
                            {
                                steeringWheel.transform.localRotation = Quaternion.Euler(defaultSteeringRotation);
                                turning = 0;
                            }
                            break;
                        case GrabPoint.GrabType.GearShift:
                            readyForNewGear = true;
                            break;
                        case GrabPoint.GrabType.Radio:
                            break;
                        case GrabPoint.GrabType.VehicleEntry:
                            break;
                        case GrabPoint.GrabType.EBrake:
                            ebraking = false;
                            break;
                        case GrabPoint.GrabType.VehicleStart:
                            break;
                        default:
                            break;
                    }

                    if (current.controller.HeldObject)
                    {
                        current.controller.HeldObject.GetComponent<Renderer>().material.color = Color.white;
                        current.controller.HeldObject = null;
                    }
                }
            }
        }
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

        if (clock) clock.enabled = false;
        if (radioName) radioName.enabled = false;
        if (volumeLevel) volumeLevel.enabled = false;
        if (SpeedUI) SpeedUI.enabled = false;
        if (GearUI) GearUI.enabled = false;


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

        if (doorIsOpen) OperateDoor();

        if (clock) clock.enabled = true;
        if (radioName) radioName.enabled = true;
        if (volumeLevel) volumeLevel.enabled = true;
        if (GearUI) GearUI.enabled = true;
        if (SpeedUI) SpeedUI.enabled = true;
                
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

    private void UpdateRadioUI()
    {
        if (RadioStation.instance != null && RadioStation.instance.songs.Count > 0 && radioSpeakers.Count > 0)
        {
            if (volumeLevel || radioName)
            {
                if (radioSpeakers[0].volume == 0 || !isOn)
                {
                    if (volumeLevel) volumeLevel.enabled = false;
                    if (radioName) radioName.enabled = false;
                }
                else
                {
                    Color minColor = Color.red;
                    Color maxColor = Color.white;

                    minColor *= 1 - radioSpeakers[0].volume;
                    maxColor *= radioSpeakers[0].volume;

                    if (volumeLevel)
                    {
                        volumeLevel.enabled = true;
                        volumeLevel.text = (radioSpeakers[0].volume * 100) + "%";
                        volumeLevel.color = minColor + maxColor;
                    }

                    if (radioName)
                    {
                        radioName.enabled = true;
                        radioName.text = "< " + RadioStation.instance.songs[songIndex].name + " >";
                    }
                }
            }
        }
    }
}
