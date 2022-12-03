using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleColorChanger : MonoBehaviour
{
    [SerializeField] bool update = false;

    [SerializeField] Material bodyMat;
    [SerializeField] Material primaryAccentMat;
    [SerializeField] Material secondaryAccentMat;
    [SerializeField] Material tireMat;

    [SerializeField] Color bodyColor;
    [SerializeField] Color primaryAccentColor;
    [SerializeField] Color secondaryAccentColor;
    [SerializeField] Color tireColor;

    private void OnValidate()
    {
        if (update)
        {
            UpdateColors(bodyColor, primaryAccentColor, secondaryAccentColor);
            update = false;

            foreach (var renderer in transform.GetComponentsInChildren(typeof(Renderer), true))
            {
                if (renderer.CompareTag("Body"))
                {
                    (renderer as Renderer).material = bodyMat;
                }
                else if (renderer.CompareTag("Accent1"))
                {
                    (renderer as Renderer).material = primaryAccentMat;
                }
                else if (renderer.CompareTag("Accent2"))
                {
                    (renderer as Renderer).material = secondaryAccentMat;
                }
                else if (renderer.CompareTag("Tire"))
                    (renderer as Renderer).material = tireMat;
            }
        }
    }

    public void UpdateColors(Color newBodyColor, Color newAccentColor, Color newSecondaryAccentColor)
    {
        bodyMat.color = newBodyColor;
        primaryAccentMat.color = newAccentColor;
        secondaryAccentMat.color = newSecondaryAccentColor;
        tireMat.color = tireColor;
    }
}
