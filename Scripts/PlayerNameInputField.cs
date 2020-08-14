using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(InputField))]
public class PlayerNameInputField : MonoBehaviour
{

    private InputField inputField;

    void Start()
    {
        string defaultName = string.Empty;
        inputField = this.GetComponent<InputField>();
		PhotonNetwork.NickName =  defaultName;
        PlayerPrefs.SetString("PlayerName",defaultName);
    }

     public void SetPlayerName(string value)
     {
        if(string.IsNullOrEmpty(value))
            return;
            
        PhotonNetwork.NickName = value;

        PlayerPrefs.SetString("PlayerName",value);
    }

    public void LimitCharacter()
    {
        inputField.characterLimit = 20;
    }
}
