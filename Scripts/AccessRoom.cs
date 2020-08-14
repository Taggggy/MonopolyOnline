using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccessRoom : MonoBehaviour
{
	public NetworkManager controller;
    public string roomName;

    void Start()
    {
    	controller = GameObject.Find("Launcher").GetComponent<NetworkManager>();
    }

    public void changeRoom(string name, string displayText)
    {
    	roomName = name;
    	gameObject.transform.Find("Text").GetComponent<Text>().text = displayText;
    	gameObject.GetComponent<Button>().onClick.AddListener(onJoiningRoom);
    }

    public void onJoiningRoom()
    {
    	controller.Join(roomName);
    }
}
