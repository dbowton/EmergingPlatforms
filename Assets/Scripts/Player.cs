using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using VRButton = UnityEngine.XR.CommonUsages;

public class Player : MonoBehaviour
{
    [SerializeField] Animator animator;

    bool canChangeCar = true;
    Vehicle car = null;
    [SerializeField] CharacterController controller;

    [SerializeField] float speed = 100f;
    [SerializeField] float spawnDistance = 2f;

    private void Start()
    {
        /*        ControllerManager.leftController.controllerObject.SetActive(false);
                ControllerManager.rightController.controllerObject.SetActive(false);*/

        vehicleMenu.enableAutoSizing = true;
        vehicleMenu.enabled = false;
    }

    [SerializeField] List<GameObject> vehicles = new List<GameObject>();
    int vehicleIndex = 0;

    [SerializeField] TMPro.TMP_Text vehicleMenu;

    bool readyToSwitchIndex = false;

    Vehicle spawnedVehicle;

    public float playerGrabRange = 0.0625f;
    private void Update()
    {
        if (car == null)
        {
            transform.localEulerAngles = Vector3.up * transform.localEulerAngles.y;
            if (ControllerManager.leftInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 dir) && dir.sqrMagnitude > 0)
            {
                Vector3 adjusedForward = Camera.main.transform.forward;
                adjusedForward.y = 0;
                adjusedForward.Normalize();

                Vector3 adjusedRight = Camera.main.transform.right;
                adjusedRight.y = 0;
                adjusedRight.Normalize();

                controller.SimpleMove(speed * Time.deltaTime * ((adjusedForward * dir.y) + (dir.x * adjusedRight)));
            }
            else
                controller.SimpleMove(Vector3.down);

            if(!vehicleMenu.enabled)
            {
                if(ControllerManager.rightInput.GetControllerPressed(VRButton.primaryButton, out bool pressed) && pressed)
                {
                    vehicleMenu.enabled = true;
                    vehicleMenu.text = "< " + vehicles[vehicleIndex].name + " >\n" + (vehicleIndex + 1) + " / " + vehicles.Count;
                }
            }
            else
            {
                if (ControllerManager.rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 choice))
                {
                    if (!readyToSwitchIndex && Mathf.Abs(choice.x) <= 0.15f)
                    {
                        readyToSwitchIndex = true;
                    }
                    else if (readyToSwitchIndex && Mathf.Abs(choice.x) > 0.9f)
                    {
                        readyToSwitchIndex = false;
                        if (choice.x > 0.9f) vehicleIndex++;
                        if (choice.x < -0.9f) vehicleIndex--;

                        if (vehicleIndex < 0) vehicleIndex = vehicles.Count - 1;
                        if (vehicleIndex >= vehicles.Count) vehicleIndex = 0;

                        vehicleMenu.text = "< " + vehicles[vehicleIndex].name + " >\n" + (vehicleIndex + 1) + " / " + vehicles.Count;
                    }
                }

                if (ControllerManager.rightInput.GetControllerPressed(VRButton.triggerButton, out bool pressed) && pressed)
                {
                    if (spawnedVehicle) Destroy(spawnedVehicle.gameObject);

                    Transform cameraT = Camera.main.transform;

                    Vector3 adjustedForward = cameraT.forward - Vector3.up * cameraT.forward.y;
                    Vector3 spawnPoint = cameraT.position + adjustedForward * spawnDistance;
                    Vector3 spawnRot = Vector3.up;

                    if(Physics.Raycast(cameraT.position, adjustedForward, out RaycastHit hitInfo, spawnDistance))
                    {
                        spawnPoint = hitInfo.point + Vector3.up * spawnDistance;
                        spawnRot = hitInfo.normal;
                    }

                    spawnedVehicle = Instantiate(vehicles[vehicleIndex], spawnPoint, Quaternion.identity).GetComponent<Vehicle>();
                    spawnedVehicle.gameObject.transform.LookAt(spawnedVehicle.transform.position + (spawnedVehicle.transform.position - transform.position), spawnRot);

                    vehicleMenu.enabled = false;
                }
            }
        }
        else
            car.UpdateVehicle(Time.deltaTime);


        if (ControllerManager.rightInput.GetControllerPressed(VRButton.gripButton, out bool leftGripped))
        {
            if (!canChangeCar && !leftGripped)
                canChangeCar = true;
            else if (canChangeCar && leftGripped)
            {
                List<Collider> grabbableObjects = Physics.OverlapSphere(ControllerManager.RightHandPos, playerGrabRange).ToList().Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && (grabPoint.grabType.Equals(GrabPoint.GrabType.VehicleEntry) || grabPoint.grabType.Equals(GrabPoint.GrabType.VehicleStart))).ToList();

                if (grabbableObjects.Count > 0)
                {
                    if (car)
                    {
                        if (car.IsStopped && grabbableObjects[0].GetComponent<GrabPoint>().grabType.Equals(GrabPoint.GrabType.VehicleStart))
                        {
                            //  leave vehicle
                            car.TurnOff();
                            car = null;
                            controller.enabled = true;
                            transform.parent = null;
                            canChangeCar = false;
                        }
                    }
                    else
                    {
                        foreach (var grab in grabbableObjects)
                        {
                            if (grabbableObjects[0].GetComponent<GrabPoint>().grabType.Equals(GrabPoint.GrabType.VehicleStart) && grab.gameObject.transform.parent.TryGetComponent<Vehicle>(out Vehicle newVehicle))
                            {
                                car = newVehicle;
                                transform.parent = car.seatPosition;
                                transform.position = car.seatPosition.position;
                                controller.enabled = false;
                                canChangeCar = false;
                                car.TurnOn();
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}