using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CardManager : MonoBehaviourPun, IPunObservable
{
	public Texture[] chanceCardTexture;
	public Texture[] communityChestCardTexture;
	public int randomNumber;

    private AudioSource drawingSound;

	void Start()
	{
		GetComponent<RawImage>().enabled = false;
        drawingSound = GameObject.Find("Audio").transform.Find("DrawingCardSound").GetComponent<AudioSource>();
	}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        	stream.SendNext(randomNumber);
        else 
        	randomNumber = (int)stream.ReceiveNext();
    }

    [PunRPC]
    public void DrawCommunityChestCard(bool transmit)
    {
    	if(transmit)
    		photonView.RPC("DrawCommunityChestCard", RpcTarget.OthersBuffered, false);
    	GetComponent<RawImage>().texture = communityChestCardTexture[randomNumber];
    	GetComponent<RawImage>().enabled = true;
    	StartCoroutine(disableCardImage());
    }

    [PunRPC]
    public void DrawChanceCard(bool transmit)
    {
    	if(transmit)
    		photonView.RPC("DrawChanceCard", RpcTarget.OthersBuffered, false);
    	GetComponent<RawImage>().texture = chanceCardTexture[randomNumber];
    	GetComponent<RawImage>().enabled = true;
    	StartCoroutine(disableCardImage());
    }

    [PunRPC]
    public void drawNewNumber(bool isChanceCard)
    {
    	if(PhotonNetwork.IsMasterClient)
    	{
    		if(isChanceCard)
    			randomNumber = Random.Range(0, chanceCardTexture.Length);
    		else
    			randomNumber = Random.Range(0, communityChestCardTexture.Length);
    	}
    	else
    		photonView.RPC("drawNewNumber", RpcTarget.MasterClient, isChanceCard);
    }

    public IEnumerator disableCardImage()
    {
        drawingSound.Play();
    	yield return new WaitForSeconds(4f);
    	GetComponent<RawImage>().enabled = false;
    }
}
