using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;

public class PlayerTurn : MonoBehaviourPun, IPunObservable
{
    public int playerTurn;
    public GameObject displayText;
    public GameObject moneyPanel;
    public GameObject doubleText;
    public GameObject waitingPlayersText;
    [HideInInspector]public bool[] isEliminated = {false, false, false, false};
    [HideInInspector]public int[] moneyWhenEliminated = {0, 0, 0, 0};

    private int numberOfPlayer;


    void Start()
    {
    	playerTurn = 1;
        displayText.SetActive(false);   
        doubleText.SetActive(false);
        applyAlpha();
    }

    public void applyAlpha()
    {
        Image ath;
        Image colorZone;
        for(int i = 0; i < moneyPanel.transform.childCount; i++)
        {
            ath = moneyPanel.transform.GetChild(i).GetChild(0).GetComponent<Image>();
            colorZone = moneyPanel.transform.GetChild(i).GetChild(1).GetComponent<Image>();
            if(i == playerTurn - 1)
            {
                ath.color = new Color(ath.color.r, ath.color.g ,ath.color.b, 1f);
                colorZone.color = new Color(colorZone.color.r, colorZone.color.g, colorZone.color.b, 1f);
            }
            else
            {
                ath.color = new Color(ath.color.r, ath.color.g ,ath.color.b, 0.25f);
                colorZone.color = new Color(colorZone.color.r, colorZone.color.g, colorZone.color.b, 0.25f);
            }
        }
    }

    [PunRPC]
    public void Next()
    {    	  
    	if(!PhotonNetwork.IsMasterClient)
    	{
    		photonView.RPC("Next", RpcTarget.MasterClient);
    	}
    	else
    	{
    		playerTurn = (playerTurn % PhotonNetwork.CurrentRoom.MaxPlayers) + 1;
            if(isEliminated[playerTurn - 1]) this.Next();
    		applyAlpha();
    	}
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    	if(stream.IsWriting)
    	{
    		stream.SendNext(playerTurn);
    	}
    	else
    	{
    		playerTurn = (int)stream.ReceiveNext();
    		applyAlpha();
    	}
    }

    [PunRPC]
    public void Display(bool transmit, string message)
    {
        displayText.SetActive(true);
        displayText.GetComponent<Text>().text = message;
        if(transmit)
            photonView.RPC("Display", RpcTarget.OthersBuffered, false, message);
    }

    [PunRPC]
    public void Double(bool transmit)
    {
        doubleText.SetActive(true);
        StartCoroutine(disableDoubleText());
        if(transmit)
            photonView.RPC("Double", RpcTarget.OthersBuffered, false);
    }

    public IEnumerator disableDoubleText()
    {
        yield return new WaitForSeconds(2f);
        doubleText.SetActive(false);
    }

    [PunRPC]
    public void Eliminated(bool transmit, int playerIndex, int moneyLeft)
    {
        isEliminated[playerIndex] = true;
        moneyWhenEliminated[playerIndex] = moneyLeft; 
        if(transmit)
            photonView.RPC("Eliminated", RpcTarget.OthersBuffered, false, playerIndex, moneyLeft);
    }

    [PunRPC]
    public void Win(bool transmit, string message)
    {
        GameObject.Find("Canvas").transform.Find("WaitingPanel").gameObject.SetActive(true);
        waitingPlayersText.SetActive(true);
        waitingPlayersText.GetComponent<Text>().text = message;
        if(transmit)
            photonView.RPC("Win", RpcTarget.OthersBuffered, false, message);
    }
}
