using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GrabPoint : MonoBehaviour
{
    public enum GrabType
    { 
        Brake,
        Steering,
        GearShift,
        Radio,
        VehicleEntry
    }

    public Vector3 defaultLocation;
    public GrabType grabType;

    public GameObject Extreme1;
    public GameObject Extreme2;


    private void Start()
    {
        defaultLocation = transform.localPosition;
    }
}
