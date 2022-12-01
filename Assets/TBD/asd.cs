using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class asd : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text text;

    float timer = 0;
    float time = 2.5f;


    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= time)
        {
            timer -= time;
            text.enabled = !text.enabled;
        }
    }
}
