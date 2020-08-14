using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundControl : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider slider;
    public Text volumeText;

    void Start()
    {
    	if(!PlayerPrefs.HasKey("MusicVolume"))
    	{
    		slider.value = 0.75f;
    		PlayerPrefs.GetFloat("MusicVolume", 0.75f);
    	}
    	else
    	{
    		slider.value = PlayerPrefs.GetFloat("MusicVolume");
    	}
    	volumeText.text = "Volume : " + (100*slider.value).ToString("N0") + "%";
    }

    public void SetLevel(float value)
    {
    	mixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
    	mixer.SetFloat("EffectVol", Mathf.Log10(value) * 20);
    	PlayerPrefs.SetFloat("MusicVolume", value);
    	volumeText.text = "Volume : " + (100*value).ToString("N0") + "%";
    }
}
