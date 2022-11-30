using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using VRButton = UnityEngine.XR.CommonUsages;

public class Player : MonoBehaviour
{
    bool canChangeCar = true;
    Vehicle car = null;
    [SerializeField] CharacterController controller;

    [SerializeField] float speed = 100f;

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

                controller.SimpleMove(((adjusedForward * dir.y) + (dir.x * adjusedRight)) * speed * Time.deltaTime);
            }
            else
                controller.SimpleMove(Vector3.down);
        }
        else
            car.UpdateVehicle();


        if (ControllerManager.rightInput.GetControllerPressed(VRButton.gripButton, out bool leftGripped))
        {
            if (!canChangeCar && !leftGripped)
                canChangeCar = true;
            else if (canChangeCar && leftGripped)
            {
                List<Collider> grabbableObjects = Physics.OverlapSphere(ControllerManager.RightHandPos, ControllerManager.playerGrabRange).ToList().Where(x => x.TryGetComponent<GrabPoint>(out GrabPoint grabPoint) && (grabPoint.grabType.Equals(GrabPoint.GrabType.VehicleEntry) || grabPoint.grabType.Equals(GrabPoint.GrabType.VehicleStart))).ToList();

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
                                transform.parent = car.transform;
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