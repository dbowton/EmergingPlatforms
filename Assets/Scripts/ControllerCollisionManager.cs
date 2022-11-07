using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerCollisionManager : MonoBehaviour
{
    public GameObject controller;

    private ControllerInitializer controllerInitializer;

    void Start() 
    {
        controllerInitializer = transform.parent.GetComponent<ControllerInitializer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(gameObject.tag))
        {
            if(controllerInitializer.heldObject != null)
            {
                controllerInitializer.heldObject.SetActive(false);
                controller.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(gameObject.tag))
        {
            if (controllerInitializer.heldObject != null)
            {
                controllerInitializer.heldObject.SetActive(true);
                controller.SetActive(false);
            }
        }
    }
}
