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

    public Color Color { get { return new Color(_sliderR.value, _sliderG.value, _sliderB.value); } }

    public void OnColorUpdated()
    {
        _color.color = Color;
    }
}
