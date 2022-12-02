using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GrabPoint : MonoBehaviour
{
    public enum GrabType
    { 
        Steering,
        GearShift,
        Radio,
        VehicleEntry,
        EBrake,
        VehicleStart
    }

    public GrabType grabType;
}
