using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseButtonControl : MonoBehaviour
{
    public GameObject menu;

    public void controlMenu()
    {
    	menu.SetActive(false);
    }
}
