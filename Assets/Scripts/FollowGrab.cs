using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowGrab : MonoBehaviour
{
    public Transform followTransform = null;
    float posOffset = 0.05f;

    void Update()
    {       
        if(followTransform != null)
        {
            Vector3 rotDif = -followTransform.forward - transform.forward;
            transform.root.forward += rotDif;

            Vector3 posDif = followTransform.position - transform.position;
            transform.root.position += posDif + followTransform.forward * posOffset;
        }
    }
}
