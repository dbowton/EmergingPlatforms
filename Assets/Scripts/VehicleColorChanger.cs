using UnityEngine;

public class VehicleColorChanger : MonoBehaviour
{
    [SerializeField] bool update = false;

    [SerializeField] Material bodyMat;
    [SerializeField] Material accentMat;
    [SerializeField] Material tireMat;

    [SerializeField] Color bodyColor;
    [SerializeField] Color accentColor;
    [SerializeField] Color tireColor;

    private void OnValidate()
    {
        if (update)
        {
            UpdateColors(bodyColor, accentColor);
            update = false;

            foreach (var renderer in transform.GetComponentsInChildren(typeof(Renderer), true))
            {
                if (renderer.CompareTag("Body"))
                {
                    (renderer as Renderer).material = bodyMat;
                }
                else if (renderer.CompareTag("Accent1"))
                {
                    (renderer as Renderer).material = accentMat;
                }
                else if (renderer.CompareTag("Tire"))
                    (renderer as Renderer).material = tireMat;
            }
        }
    }

    public void UpdateColors(Color newBodyColor, Color newAccentColor)
    {
        bodyMat.color = newBodyColor;
        accentMat.color = newAccentColor;
        tireMat.color = tireColor;
    }
}
