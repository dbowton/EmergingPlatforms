using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayCycle : MonoBehaviour
{    
    [SerializeField] int dayTime;
    private float currentTime;

    [SerializeField] Material dayMaterial;
    [SerializeField] Material nightMaterial;

    bool isDay = true;

    void Update()
    {
        transform.Rotate((360f / dayTime) * Time.deltaTime * Vector3.right);

        return;
        currentTime += Time.deltaTime;
        currentTime %= dayTime;

        if(currentTime > Mathf.Abs(currentTime - dayTime))
        {
            if(!isDay)
            {
                isDay = true;
                RenderSettings.skybox = nightMaterial;
            }
        }
        else
        {
            if (isDay)
            {
                isDay = false;
                RenderSettings.skybox = dayMaterial;
            }
        }

    }
}
