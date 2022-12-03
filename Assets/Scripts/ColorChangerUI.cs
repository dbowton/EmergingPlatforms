using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChangerUI : MonoBehaviour
{
    [SerializeField] private Slider _sliderR;
    [SerializeField] private Slider _sliderG;
    [SerializeField] private Slider _sliderB;

    [SerializeField] private Image _color;

    public void OnColorUpdated()
    {
        Color c = new Color(_sliderR.value, _sliderG.value, _sliderB.value);
        _color.color = c;
    }
}
