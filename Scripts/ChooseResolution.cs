using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseResolution : MonoBehaviour
{
	public FullScreenControl windowed;

	private bool start = true;

	void Start()
	{
		if(PlayerPrefs.HasKey("Width") && PlayerPrefs.HasKey("Height"))
		{
			if(PlayerPrefs.HasKey("Fullscreen"))
				Screen.SetResolution(PlayerPrefs.GetInt("Width"), PlayerPrefs.GetInt("Height"), (PlayerPrefs.GetInt("Fullscreen") != 0));
			else
				Screen.SetResolution(PlayerPrefs.GetInt("Width"), PlayerPrefs.GetInt("Height"), true);

			switch(PlayerPrefs.GetInt("Width"))
			{
				case(1920): gameObject.GetComponent<Dropdown>().value = 0; break;
				case(1600): gameObject.GetComponent<Dropdown>().value = 1; break;
				case(1280): gameObject.GetComponent<Dropdown>().value = 2; break;
				case(1152): gameObject.GetComponent<Dropdown>().value = 3; break;
				case(1024): gameObject.GetComponent<Dropdown>().value = 4; break;
				case(960): gameObject.GetComponent<Dropdown>().value = 5; break;
				case(640): gameObject.GetComponent<Dropdown>().value = 6; break;
			}
		}
		else
		{
			if(PlayerPrefs.HasKey("Fullscreen"))
				Screen.SetResolution(1920, 1080, (PlayerPrefs.GetInt("Fullscreen") != 0));
			else
				Screen.SetResolution(1920, 1080, true);
		}
		start = false;
	}


    public void OnValueChanged(Dropdown dropDown)
    {
    	if(start)
    		return;
    	int width = 1920;
    	int height = 1080;
    	switch(dropDown.value)
    	{
    		case(0): break;
    		case(1): width = 1600; height = 900; break;
    		case(2): width = 1280; height = 720; break;
    		case(3): width = 1152; height = 648; break;
    		case(4): width = 1024; height = 576; break;
    		case(5): width = 960; height = 540; break;
    		case(6): width = 640; height = 360; break;
    	}
    	Screen.SetResolution(width, height, windowed.fullscreen); 
    	PlayerPrefs.SetInt("Width", width);
    	PlayerPrefs.SetInt("Height", height);
    }
}