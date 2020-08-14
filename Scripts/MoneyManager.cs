using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Globalization;

public class MoneyManager : MonoBehaviourPun, IPunObservable
{
	public int currentMoney;
	public Text moneyText;
	public Image imageColor;
	public AudioSource audioDataPlus;
	public AudioSource audioDataMinus;

	private NumberFormatInfo nfi;
	[SerializeField] private int startMoney = 2000000;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    	if(stream.IsWriting)
    	{
    		stream.SendNext(currentMoney);
    	}
    	else
    	{
    		currentMoney = (int)stream.ReceiveNext();
    		moneyText.text = currentMoney.ToString("n", nfi) + "€";  
    	}
    }

    void Start()
    {
    	nfi = new NumberFormatInfo {NumberGroupSeparator = " ", NumberDecimalDigits = 0};
    	currentMoney = startMoney;
    	moneyText.text = currentMoney.ToString("n", nfi) + "€"; 
    	gameObject.transform.GetChild(3).GetComponent<Text>().text = string.Empty;
    }

    [PunRPC]
    public void Spend(bool transmit, int money)
    {
    	currentMoney += money;
    	photonView.RPC("updateMoney", RpcTarget.AllBuffered, money);
        moneyText.text = currentMoney.ToString("n", nfi) + "€"; 
        if(transmit)
            photonView.RPC("Spend", RpcTarget.OthersBuffered, false, money);
    }

    [PunRPC]
    public void updateMoney(int money)
    {
    	Text display = gameObject.transform.GetChild(3).GetComponent<Text>();
    	if(money > 0)
    	{
    		display.color = Color.green;
    		display.text = "+" + money.ToString("n", nfi) + "€";
    		if(!audioDataPlus.isPlaying)
    			audioDataPlus.Play();
    	}
    	else if(money < 0)
    	{
    		display.color = Color.red;
    		display.text = money.ToString("n", nfi) + "€";
    		if(!audioDataMinus.isPlaying)
    			audioDataMinus.Play();
    	}
    	StartCoroutine(disableUpdateText());
    }

    public IEnumerator disableUpdateText()
    {
    	Text display = gameObject.transform.GetChild(3).GetComponent<Text>();
    	yield return new WaitForSeconds(2f);
    	display.text = string.Empty;
    }
}
