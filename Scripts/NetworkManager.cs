using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {

    public GameObject controlPanel;
    public GameObject progressLabel;
    public GameObject settingsPanel;
    public GameObject roomPanel;
    public GameObject roomButtonPrefab;

    private bool hasEnteredNickname = false;
    private bool readyToConnect = false;
    private bool roomAvailable = false;
    private Button createRoomButton;
    private Button joinRoomButton;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    
    void Start() 
    {
    	progressLabel.SetActive(false);
		controlPanel.SetActive(true);
        settingsPanel.SetActive(false);

		if(!PhotonNetwork.IsConnected)
			PhotonNetwork.ConnectUsingSettings();

        createRoomButton = controlPanel.transform.Find("CreateRoomButton").GetComponent<Button>();
        joinRoomButton = controlPanel.transform.Find("JoinRoomButton").GetComponent<Button>();

        createRoomButton.onClick.AddListener(creatingRoom);
    }

    public void Connect()
    {
    	progressLabel.SetActive(true);
		controlPanel.SetActive(false);
        settingsPanel.SetActive(false);
		PhotonNetwork.LeaveLobby();
        if(PhotonNetwork.IsConnected)
        {
        	RoomOptions roomOptions = new RoomOptions();
        	roomOptions.IsOpen = true;
        	roomOptions.IsVisible = true;
        	roomOptions.MaxPlayers = (byte)settingsPanel.transform.Find("PlayerNumberSlider").GetComponent<Slider>().value;
            string roomName = settingsPanel.transform.Find("RoomName InputField").GetComponent<InputField>().text;
            PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
        }
        else
            PhotonNetwork.ConnectUsingSettings();
    }

    public void Join(string roomName)
    {
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);
        settingsPanel.SetActive(false);
        roomPanel.SetActive(false);
        PhotonNetwork.LeaveLobby();
        if(PhotonNetwork.IsConnected)
            PhotonNetwork.JoinRoom(roomName);
        else
            PhotonNetwork.ConnectUsingSettings();
    }

    public void nickName(string value)
    {
        if(value.Equals(string.Empty))
        {
            hasEnteredNickname = false;
            hideRoomSettings();
        }
        else
            hasEnteredNickname = true;
        activateButton();
    }

    public void activateButton()
    {
        if(readyToConnect && hasEnteredNickname)
        {
            createRoomButton.interactable = true;
            if(roomAvailable)
                joinRoomButton.interactable = true;
        }
        else
        {
            createRoomButton.interactable = false;
            joinRoomButton.interactable = false;
        }
    }

    public override void OnConnectedToMaster()
	{
        readyToConnect = true;
	    activateButton();
	    PhotonNetwork.JoinLobby();
	}
	public override void OnJoinedRoom()
	{
	    PhotonNetwork.LoadLevel("BoardScene");
	}

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for(int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if(info.RemovedFromList || info.PlayerCount == info.MaxPlayers)
                cachedRoomList.Remove(info.Name);
            else
                cachedRoomList[info.Name] = info;
        }
    }


	public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomAvailable = false;

        RectTransform content = roomPanel.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();

        UpdateCachedRoomList(roomList);
        if(cachedRoomList.Count > 0) roomAvailable = true;
        else roomAvailable = false;

        clearRoom();

        if(roomAvailable)
        {
            if(hasEnteredNickname)
                joinRoomButton.interactable = true;

            content.sizeDelta = new Vector2(0, 40*(cachedRoomList.Count + 1));
            RectTransform titleTransform = content.Find("Title").GetComponent<RectTransform>();
            titleTransform.anchoredPosition = new Vector3(8.5f, 40*(cachedRoomList.Count + 1)/2 - 25/2, 0f);
            RectTransform closeTransform = content.Find("Close").GetComponent<RectTransform>();
            closeTransform.anchoredPosition = new Vector3(-80f, 40*(cachedRoomList.Count + 1)/2 - 25/2, 0f);

            GameObject[] joinButton = new GameObject[cachedRoomList.Count];
            Vector3 formerPosition;

            joinButton[0] = Instantiate(roomButtonPrefab, new Vector3(8.5f, titleTransform.anchoredPosition.y - 32.5f, 0f), Quaternion.identity) as GameObject;
            formerPosition = joinButton[0].GetComponent<RectTransform>().position;
            joinButton[0].GetComponent<RectTransform>().SetParent(content);
            joinButton[0].GetComponent<RectTransform>().anchoredPosition = formerPosition;
            joinButton[0].GetComponent<RectTransform>().localScale = Vector3.one;
            joinButton[0].GetComponent<RectTransform>().sizeDelta = new Vector2(110f, 35f);

            for(int i = 1; i < cachedRoomList.Count; i++)
            {
                joinButton[i] = Instantiate(roomButtonPrefab, new Vector3(8.5f, titleTransform.anchoredPosition.y - 32.5f - (40f*i), 0f), Quaternion.identity) as GameObject;
                formerPosition = joinButton[i].GetComponent<RectTransform>().position;
                joinButton[i].GetComponent<RectTransform>().SetParent(content);
                joinButton[i].GetComponent<RectTransform>().anchoredPosition = formerPosition;
                joinButton[i].GetComponent<RectTransform>().localScale = Vector3.one;
                joinButton[i].GetComponent<RectTransform>().sizeDelta = new Vector2(110f, 35f);
            }

            int index = 0;

            foreach(var room in cachedRoomList)
                joinButton[index++].GetComponent<AccessRoom>().changeRoom(room.Key, "<b>" + room.Key + "</b>\n<i>" + room.Value.PlayerCount +  "/" + room.Value.MaxPlayers + "</i>");
        }
        else
        {
            joinRoomButton.interactable = false;
        }  
    }

    public void creatingRoom()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void changeCreateButton(string value)
    {
        Text display = createRoomButton.transform.GetChild(0).GetComponent<Text>();
        if(value == string.Empty)
        {
            createRoomButton.interactable = false;
            if(hasEnteredNickname)
                display.text = "Choix d'un nom";

            createRoomButton.onClick.RemoveAllListeners();
            createRoomButton.onClick.AddListener(creatingRoom);
        }
        else
        {
            createRoomButton.interactable = true;
            display.text = "Lancer la partie";

            createRoomButton.onClick.RemoveAllListeners();
            createRoomButton.onClick.AddListener(Connect);
        }
    }

    public void clearRoom()
    {
        RectTransform content = roomPanel.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();
        for(int i = content.childCount - 1; i > 1; i--)
        {
            Transform child = content.transform.GetChild(i);
            child.SetParent(null);
        }
    }

    public void hideRoomSettings()
    {
        settingsPanel.transform.GetChild(1).GetComponent<InputField>().text = " ";
        createRoomButton.transform.GetChild(0).GetComponent<Text>().text = "Créer une partie";
        createRoomButton.onClick.RemoveAllListeners();
        createRoomButton.onClick.AddListener(creatingRoom);
        settingsPanel.SetActive(false);
    }
}