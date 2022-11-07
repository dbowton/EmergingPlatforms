using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using InputDevice = UnityEngine.XR.InputDevice;

public class Player : MonoBehaviour
{
    [SerializeField] ControllerInitializer leftController;
    [SerializeField] ControllerInitializer rightController;

    [SerializeField] InputDevice leftInput;
    [SerializeField] InputDevice rightInput;

    [SerializeField] float speed = 4;
    [SerializeField] CharacterController characterController;
    [SerializeField] PlayerInput playerInput;

    [SerializeField] LineRenderer leftLineRenderer;
    [SerializeField] LineRenderer rightLineRenderer;

    [SerializeField] AudioSource audioSource;

    private Vector2 movement;

    private bool fireLeft = false;
    private bool fireRight = false;

    public Vector3 leftHandPos { get { return leftController.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.transform.position; } }
    public Vector3 rightHandPos { get { return rightController.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.transform.position; } }

    void Start() 
    {
        var leftHandedControllers = new List<UnityEngine.XR.InputDevice>();
        var desiredLeftCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredLeftCharacteristics, leftHandedControllers);

        var rightHandedControllers = new List<UnityEngine.XR.InputDevice>();
        var desiredRightCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredRightCharacteristics, rightHandedControllers);

        leftInput = leftHandedControllers[0];
        rightInput = rightHandedControllers[0];
    }

    private void Update()
    {
        Vector3 dir = movement.x * Camera.main.transform.right + movement.y * Camera.main.transform.forward;
        dir.y = 0;

        characterController.SimpleMove(speed * dir);

        DrawLine(rightLineRenderer, rightController, leftController);
        DrawLine(leftLineRenderer, leftController, rightController);

        if (leftInput.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool leftGrabVal))
        {
            if (leftGrabVal && leftController.heldObject == null)
                Grab(leftController, rightController);
            else if (!leftGrabVal && leftController.heldObject != null)
                Drop(leftController);
        }

        if (rightInput.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool rightGrabVal))
        {
            if (rightGrabVal && rightController.heldObject == null)
                Grab(rightController, leftController);
            else if (!rightGrabVal && rightController.heldObject != null)
                Drop(rightController);
        }

        if (leftInput.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool leftFireVal))
        {
/*            if (leftController.heldObject != null && leftController.heldObject.TryGetComponent<Gun>(out Gun leftGun))
            {
                if (leftFireVal)
                {
                    if (leftGun.isFullAuto)
                        leftGun.Fire();
                    else if (fireLeft)
                    {
                        leftGun.Fire();
                        fireLeft = false;
                    }
                }
                else
                    fireLeft = true;
            }*/
        }

        if (rightInput.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool rightFireVal))
        {
/*            if (rightController.heldObject != null && rightController.heldObject.TryGetComponent<Gun>(out Gun rightGun))
            {
                if (rightFireVal)
                {
                    if (rightGun.isFullAuto)
                        rightGun.Fire();
                    else if (fireRight)
                    {
                        rightGun.Fire();
                        fireRight = false;
                    }
                }
                else
                    fireRight = true;
            }
*/        }
    }

    public void OnDeath() 
    {
        SceneManager.LoadScene("GameOver");
    }

    public void DrawLine(LineRenderer renderer, ControllerInitializer controller, ControllerInitializer other)
    {
/*        if(controller.heldObject != null)
        {
            controller.grabRenderer.enabled = false;
            if(controller.heldObject.TryGetComponent<Gun>(out Gun gun))
            {
                renderer.enabled = true;

                Color color;
                Ray ray = new Ray(gun.gunPort.position, gun.gunPort.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, gun.range))
                {
                    if (hit.collider.gameObject != gameObject && hit.collider.gameObject.transform.root.TryGetComponent<Health>(out Health health))
                    {
                        color = Color.red;
                    }
                    else
                        color = Color.green;
                }
                else
                    color = Color.green;


                renderer.startColor = color;
                renderer.endColor = color;

                renderer.SetPosition(0, gun.gunPort.position);
                renderer.SetPosition(1, gun.gunPort.position + gun.range * gun.gunPort.forward);
            }
            else
                renderer.enabled = false;
        }
        else
        {
            renderer.enabled = false;

            controller.grabRenderer.enabled = true;

            Collider[] grabbedObjects = Physics.OverlapSphere(controller.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.transform.position, 0.125f, 1 << LayerMask.NameToLayer("Pickup"));

            if (grabbedObjects.Length > 0 && grabbedObjects[0].gameObject != other.heldObject)
                    controller.grabRenderer.material.SetColor("_Color", Colors.darkRed_t);
            else
                controller.grabRenderer.material.SetColor("_Color", Colors.lightBlue_t);
        }*/
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        return;
//        movement = context.ReadValue<Vector2>();
    }

    public void Grab(ControllerInitializer active, ControllerInitializer other)
    {
        Collider[] grabbedObjects = Physics.OverlapSphere(active.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.transform.position, 0.125f, 1 << LayerMask.NameToLayer("Pickup"));

        if (grabbedObjects.Length > 0)
        {
            GameObject pickupObject = grabbedObjects[0].gameObject;

            /*            if (pickupObject.TryGetComponent<Gun>(out _))
                        {
                            if (pickupObject == other.heldObject) return;
                            active.heldObject = pickupObject;

                            pickupObject.GetComponent<Rigidbody>().isKinematic = true;
                            pickupObject.GetComponent<Rigidbody>().useGravity = false;

                            pickupObject.transform.parent = active.gameObject.transform.parent;
                            active.heldObject.transform.localPosition = Vector3.zero;
                            active.heldObject.transform.localEulerAngles = Vector3.up * 90;

                            active.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.SetActive(false);
                        }
                        else
                        {
                            active.heldObject = pickupObject;
                            if (other.heldObject != null && pickupObject.transform.root == other.heldObject.transform.root)
                            {
                                if (other.heldObject.TryGetComponent<FollowGrab>(out FollowGrab follow))
                                    Destroy(follow);

                                other.heldObject = null;
                                active.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.SetActive(true);
                            }

                            FollowGrab followGrab = pickupObject.AddComponent<FollowGrab>();
                            followGrab.followTransform = active.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.transform;

                            Collider[] colliders = pickupObject.transform.root.GetComponentsInChildren<Collider>();
                            foreach (var c in colliders)
                                c.isTrigger = true;
                        }*/
        }
    }

        public void Drop(ControllerInitializer active)
    {
/*        if(active.heldObject.TryGetComponent<Gun>(out _))
        {
            active.heldObject.layer = LayerMask.NameToLayer("Default");
            active.heldObject.transform.parent = null;

            active.heldObject.GetComponent<Rigidbody>().isKinematic = false;
            active.heldObject.GetComponent<Rigidbody>().useGravity = true;

            active.heldObject = null;
            active.controllerPrefab.GetComponent<ControllerCollisionManager>().controller.SetActive(true);
        }
        else
        {
            if(active.heldObject.TryGetComponent<FollowGrab>(out FollowGrab followGrab))
            {
                Destroy(followGrab);
               
                if(active.heldObject.transform.root.TryGetComponent<Enemy>(out Enemy enemy))
                    enemy.active = true;
                
                Collider[] colliders = active.heldObject.GetComponentsInChildren<Collider>();
            
                foreach (var c in colliders)
                {
                    if(!c.gameObject.CompareTag("hitBox"))
                        c.isTrigger = false;
                }
            }
        }
       
        active.heldObject = null;*/
    }
}
