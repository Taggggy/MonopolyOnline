using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{

	public GameObject playerPrefab;
	[HideInInspector] public int playerNumber;
    [HideInInspector] public bool start = false;
    public Text waitingPlayers;

	public int playerInGame = 1;
	

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    void Start()
    {
        playerNumber = PhotonNetwork.CurrentRoom.PlayerCount;
        waitingPlayers.text = "En attente de joueurs (" + playerNumber + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";

        if(PhotonNetwork.CurrentRoom.MaxPlayers == 1 || PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers) //Pour pouvoir tester en solo
        {
            start = true;
            GameObject.Find("Canvas").transform.Find("WaitingPanel").gameObject.SetActive(false);
        }

        PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(-4.25f,0.25f,4.25f), Quaternion.identity, 0);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(1);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.IsMasterClient)
        	playerInGame++;

        if(PhotonNetwork.PlayerList.Length == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
        	start = true;
            GameObject.Find("Canvas").transform.Find("WaitingPanel").gameObject.SetActive(false);
        }
        else
            waitingPlayers.text = "En attente de joueurs (" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(PhotonNetwork.IsMasterClient)
        	playerInGame--;

        start = false;
        waitingPlayers.text = "En attente de joueurs (" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";
    }
}
