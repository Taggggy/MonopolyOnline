using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class PlayerNumberSlider : MonoBehaviour
{
    public int playerNumber;

    void Start()
    {
        playerNumber = 4;
    }

     public void SetPlayerNumber()
     {
        playerNumber = (int)GetComponent<Slider>().value;
        this.transform.Find("Text").GetComponent<Text>().text = "Nombre de joueur : " + playerNumber;
    }
}
