using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleTester : MonoBehaviour
{
    [SerializeField] Transform simulatedWheelCenter;
    [SerializeField] Transform simulatedGrabPoint;
    [SerializeField] Transform simulatedHandPos;
    [SerializeField] GameObject simulatedWheel;

    GameObject adjustedHand;

    float turning = 0;

    private void Start()
    {
        adjustedHand = Instantiate(simulatedHandPos.gameObject);
    }

    void Update()
    {
        Vector3 baseVector = simulatedGrabPoint.position - simulatedWheelCenter.position;
        Vector3 angledVector = simulatedHandPos.position - simulatedWheelCenter.position;

        angledVector = simulatedWheelCenter.position + Vector3.ProjectOnPlane(angledVector, simulatedWheelCenter.up);

        adjustedHand.transform.position = angledVector;

        turning = Vector3.SignedAngle(baseVector, angledVector - simulatedWheelCenter.position, simulatedWheelCenter.up);

        Vector3 rot = simulatedWheel.transform.localEulerAngles;
        simulatedWheel.transform.localRotation = Quaternion.Euler(rot.x, turning, rot.z);

        Debug.DrawLine(simulatedWheelCenter.position, simulatedGrabPoint.position, Color.blue);
        Debug.DrawLine(simulatedWheelCenter.position, simulatedHandPos.position, Color.green);
        Debug.DrawLine(simulatedWheelCenter.position, simulatedWheelCenter.position + simulatedWheelCenter.up, Color.red);

    }
}
