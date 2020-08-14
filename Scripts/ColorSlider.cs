using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSlider : MonoBehaviour
{
    public Slider slider;
    public Image sliderHandle;
    public Image sliderBg;

    private Texture2D colorTex;

    void Awake()
    {
    	colorTex = ColorStrip(360);

    	Rect rect = new Rect(0, 0, colorTex.width, colorTex.height);

    	slider.onValueChanged.AddListener(OnValueChanged);
    	sliderBg.sprite = Sprite.Create(colorTex, rect, rect.center);

    	OnValueChanged(slider.value);
    }

    private void OnValueChanged(float value)
    {
    	sliderHandle.color = Color.HSVToRGB(value, 1, 1);
    }
 
    private Texture2D ColorStrip (int density)
    {
        Texture2D hueTex = new Texture2D (density, 1);
 
        Color[] colors = new Color[density];
        for(int i = 0; i < density; i++)
        	colors[i] = Color.HSVToRGB ((1f * i) / density, 1, 1);
 
        hueTex.SetPixels(colors);
        hueTex.Apply();
 
        return hueTex;
    }

    void OnDisable()
    {
    	PlayerPrefs.SetString("color", ColorUtility.ToHtmlStringRGB(sliderHandle.color));
    }
}
