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
        menu = menuObject.GetComponent<VehicleSpawnMenu>();
    }

    [SerializeField] List<GameObject> vehicles = new List<GameObject>();
    int vehicleIndex = 0;

    bool readyToSwitchIndex = false;

    Vehicle spawnedVehicle;

    public float playerGrabRange = 0.0625f;

    public bool canOpenDoor = true;

    public bool inMenu = false;
    public GameObject menuObject;
    private VehicleSpawnMenu menu;

    private int menuState = 0;


    private int selectedType = 0;
    private int selectedSlider = 0;

    [SerializeField] Material bodyMat;
    [SerializeField] Material accentMat;
    [SerializeField] Material tireMat;

    private bool prevPressed = false;

    private void Update()
    {
        if (!inMenu && car == null && ControllerManager.rightInput.GetControllerPressed(VRButton.primaryButton, out bool startMenu) && startMenu)
        {
            prevPressed = true;
            inMenu = true;

            menu.spawnObject.SetActive(true);
            menu.customizeObject.SetActive(false);
            menu.colorObject.SetActive(false);

            menuState = 0;

            menu.vehicleName.text = vehicles[vehicleIndex].name + "\n" + (vehicleIndex + 1) + " / " + vehicles.Count;
            return;
        }

        if (inMenu)
        {
            if (prevPressed)
            {
                prevPressed = (ControllerManager.rightInput.GetControllerPressed(VRButton.primaryButton, out bool primaryPressed) && primaryPressed) ||
                    (ControllerManager.rightInput.GetControllerPressed(VRButton.secondaryButton, out bool secondaryPressed) && secondaryPressed);

                return;
            }

            switch (menuState)
            {
                case 0: // vehicleSpawn
                    {
                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.triggerButton, out bool spawnVehcile) && spawnVehcile)
                        {
                            if (spawnedVehicle) Destroy(spawnedVehicle.gameObject);

                            Transform cameraT = Camera.main.transform;

                            Vector3 adjustedForward = cameraT.forward - Vector3.up * cameraT.forward.y;
                            Vector3 spawnPoint = cameraT.position + adjustedForward * spawnDistance;
                            Vector3 spawnRot = Vector3.up;

                            if (Physics.Raycast(cameraT.position, adjustedForward, out RaycastHit hitInfo, spawnDistance))
                            {
                                spawnPoint = hitInfo.point + Vector3.up * spawnDistance;
                                spawnRot = hitInfo.normal;
                            }

                            spawnedVehicle = Instantiate(vehicles[vehicleIndex], spawnPoint, Quaternion.identity).GetComponent<Vehicle>();
                            spawnedVehicle.gameObject.transform.LookAt(spawnedVehicle.transform.position + (spawnedVehicle.transform.position - transform.position), spawnRot);

                            inMenu = false;
                            menu.spawnObject.SetActive(false);
                            return;
                        }

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

                                menu.vehicleName.text = "< " + vehicles[vehicleIndex].name + " >\n" + (vehicleIndex + 1) + " / " + vehicles.Count;
                                return;
                            }
                        }

                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.primaryButton, out bool cusomize) && cusomize)
                        {
                            prevPressed = true;

                            menuState = 1;

                            menu.spawnObject.SetActive(false);
                            menu.customizeObject.SetActive(true);
                            return;
                        }

                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.secondaryButton, out bool back) && back)
                        {
                            prevPressed = true;

                            inMenu = false;
                            menu.spawnObject.SetActive(false);
                            return;
                        }
                    }
                    break;
                case 1: // selectType
                    {
                        menu.bodyText.color = (selectedType == 0) ? Color.black : Color.white;
                        menu.accentText.color = (selectedType == 1) ? Color.black : Color.white;
                        menu.tireText.color = (selectedType == 2) ? Color.black : Color.white;

                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 changeType))
                        {
                            if (!readyToSwitchIndex && Mathf.Abs(changeType.y) <= 0.15f)
                            {
                                readyToSwitchIndex = true;
                            }
                            else if (readyToSwitchIndex && Mathf.Abs(changeType.y) > 0.9f)
                            {
                                readyToSwitchIndex = false;
                                if (changeType.y > 0.9f) selectedType--;
                                else if (changeType.y < -0.9f) selectedType++;

                                if (selectedType < 0) selectedType = 2;
                                else if (selectedType > 2) selectedType = 0;

                                return;
                            }
                        }

                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.primaryButton, out bool selectType) && selectType)
                        {
                            prevPressed = true;
                            menuState = 2;

                            menu.customizeObject.SetActive(false);
                            menu.colorObject.SetActive(true);

                            selectedSlider = 0;

                            return;
                        }

                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.secondaryButton, out bool back) && back)
                        {
                            prevPressed = true;
                            menuState = 0;

                            menu.spawnObject.SetActive(true);
                            menu.customizeObject.SetActive(false);

                            return;
                        }

                    }
                    break;
                case 2: // customizeColor
                    {
                        Material mat = bodyMat;

                        if (selectedType == 0) mat = bodyMat;
                        if (selectedType == 1) mat = accentMat;
                        if (selectedType == 2) mat = tireMat;

                        menu.rSlider.value = mat.color.r;
                        menu.gSlider.value = mat.color.g;
                        menu.bSlider.value = mat.color.b;

                        menu.sliderColor.material = mat;
                        menu.colorType.text = (selectedType == 0) ? "Body Color" : (selectedType == 1) ? "Accent Color" : "Tire Color";

                        UnityEngine.UI.ColorBlock block = menu.rSlider.colors;
                        block.normalColor = (selectedSlider == 0) ? Color.black : Color.white;

                        menu.rSlider.colors = block;

                        block = menu.gSlider.colors;
                        block.normalColor = (selectedSlider == 1) ? Color.black : Color.white;

                        menu.gSlider.colors = block;

                        block = menu.bSlider.colors;
                        block.normalColor = (selectedSlider == 2) ? Color.black : Color.white;

                        menu.bSlider.colors = block;

                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.primary2DAxis, out Vector2 movement))
                        {
                            if (!readyToSwitchIndex && movement.magnitude <= 0.15f)
                            {
                                readyToSwitchIndex = true;
                            }
                            else if (readyToSwitchIndex && movement.magnitude > 0.9f)
                            {
                                readyToSwitchIndex = false;

                                if (movement.y > 0.9f)
                                {
                                    selectedSlider--;
                                    if (selectedSlider < 0) selectedSlider = 2;

                                    return;
                                }
                                if (movement.y < -0.9f)
                                {
                                    selectedSlider++;
                                    if (selectedSlider > 2) selectedSlider = 0;

                                    return;
                                }

                                if (movement.x > 0.9f || movement.x < -0.9f)
                                {
                                    Color newColor = mat.color;

                                    float change = (movement.x > 0.9f) ? 0.1f : -0.1f;

                                    if (selectedSlider == 0) newColor.r = Mathf.Max(0, Mathf.Min(1, mat.color.r + change));
                                    if (selectedSlider == 1) newColor.g = Mathf.Max(0, Mathf.Min(1, mat.color.g + change));
                                    if (selectedSlider == 2) newColor.b = Mathf.Max(0, Mathf.Min(1, mat.color.b + change));

                                    mat.color = newColor;
                                    return;
                                }


                                return;
                            }
                        }

                        if (ControllerManager.rightInput.GetControllerPressed(VRButton.secondaryButton, out bool back) && back)
                        {
                            prevPressed = true;
                            menuState = 1;

                            menu.colorObject.SetActive(false);
                            menu.customizeObject.SetActive(true);

                            return;
                        }

                    }
                    break;

                default:
                    break;
            }

            return;
        }
        else prevPressed = false;

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
            {
                controller.SimpleMove(Vector3.down);
            }
        }
        else
            car.UpdateVehicle(Time.deltaTime);

        if (ControllerManager.rightInput.GetControllerPressed(VRButton.gripButton, out bool rightGrip))
        {
            if (!rightGrip) canOpenDoor = true;
            if (!canChangeCar && !rightGrip)
                canChangeCar = true;
            else if (canChangeCar && rightGrip)
            {
                List<Collider> grabbableObjects = Physics.OverlapSphere(ControllerManager.RightHandPos, playerGrabRange)
                    .Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && 
                        (grabPoint.grabType.Equals(GrabPoint.GrabType.VehicleEntry) || grabPoint.grabType.Equals(GrabPoint.GrabType.VehicleStart)))
                    .OrderBy(x => Vector3.Distance(x.transform.position, ControllerManager.RightHandPos)).ToList();

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
                            if(grab.gameObject.transform.parent.TryGetComponent<Vehicle>(out Vehicle workingVehicle))
                            {
                                if(canOpenDoor && grabbableObjects[0].GetComponent<GrabPoint>().grabType.Equals(GrabPoint.GrabType.VehicleEntry))
                                {
                                    workingVehicle.OperateDoor();
                                    canOpenDoor = false;
                                }
                                else if (grabbableObjects[0].GetComponent<GrabPoint>().grabType.Equals(GrabPoint.GrabType.VehicleStart))
                                {
                                    car = workingVehicle;
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
}