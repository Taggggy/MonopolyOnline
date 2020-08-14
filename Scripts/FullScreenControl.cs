using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenControl : MonoBehaviour
{
	public ChooseResolution changeResolution;
	[HideInInspector] public bool fullscreen;

	void Start()
	{
		if(PlayerPrefs.HasKey("Fullscreen"))
		{
			fullscreen = (PlayerPrefs.GetInt("Fullscreen") != 0);
			gameObject.GetComponent<Toggle>().isOn = fullscreen;
		}
		else
			fullscreen = true;

	}

    public void OnValueChanged(Toggle change)
    {
    	fullscreen = change.isOn;
    	changeResolution.OnValueChanged(changeResolution.gameObject.GetComponent<Dropdown>());
    	PlayerPrefs.SetInt("Fullscreen", (fullscreen ? 1 : 0));
    }
}
