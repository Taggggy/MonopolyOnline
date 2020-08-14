using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class RoomNameInputField : MonoBehaviour
{
	[HideInInspector] public string roomName;

    public InputField inputField;

    void Start()
    {
        roomName = string.Empty;
    }

    public void SetRoomName(string value)
    {
        if(string.IsNullOrEmpty(value))
            return;
            
        roomName = value;
    }

    public void LimitCharacter()
    {
        inputField.characterLimit = 20;
    }
}
