using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

using Photon.Pun;

public class PlayerManager : MonoBehaviourPun, IPunObservable
{
	public GameObject PlayerUIPrefab;
	public GameObject explosionPrefab;

	private int speed = 3;
	private int position = 0;
	private int actualPosition = 0;
	private bool turn;
	private bool inPrison = false;
	private int turnInPrison = 0;
	private int consecutiveDouble = 0;
	private int freePrisonExit = 0;
	private bool drawingCard;

	private PlayerTurn playerTurnScript;
	private GameObject UI;
	private PlayerUI playerUI;
	private GameManager gameManager;
	private MoneyManager moneyManager;
	private CardManager cardManager;
	private NumberFormatInfo nfi;

	private int playerTurn;
	private float[,] grid = {
								{-4.25f, 4.25f}, {-3.25f, 4.25f}, {-2.45f, 4.25f}, {-1.65f, 4.25f}, {-0.85f, 4.25f}, {0f, 4.25f}, {0.8f, 4.25f}, {1.6f, 4.25f}, {2.45f, 4.25f}, {3.25f, 4.25f}, {4.25f, 4.25f},
								{4.25f, 3.25f}, {4.25f, 2.45f}, {4.25f, 1.65f}, {4.25f, 0.85f}, {4.25f, 0f}, {4.25f, -0.8f}, {4.25f, -1.6f}, {4.25f, -2.45f}, {4.25f, -3.25f}, {4.25f, -4.25f},
								{3.25f, -4.25f}, {2.45f, -4.25f}, {1.65f, -4.25f}, {0.85f, -4.25f}, {0f, -4.25f}, {-0.8f, -4.25f}, {-1.6f, -4.25f}, {-2.45f, -4.25f}, {-3.25f, -4.25f}, {-4.25f, -4.25f},
								{-4.25f, -3.25f}, {-4.25f, -2.45f}, {-4.25f, -1.65f}, {-4.25f, -0.85f}, {-4.25f, 0f}, {-4.25f, 0.8f}, {-4.25f, 1.6f}, {-4.25f, 2.45f}, {-4.25f, 3.25f}
							};
	private string[] buttonText = {"Acheter", "1 maison", "2 maisons", "3 maisons", "Hôtel", "Ne pas acheter"};
	private Vector3 newPosition;
	private RollDice[] dices = {null, null};
	private bool tryToEscape = false;
	private bool moneyManagerDeactivated = false;
	private int buyChoice = -2;
	private PropertyManager targettedProperty = null;

	private bool[] soldProperties;
	private int validateSelling = 0;
	private int somme = 0;
	
	private static bool giveMoneyOutsideTurn = false;
	private static int playerToPay = 0;
	private static int moneyToPay = -100000000;
	private bool toMonumentFromChanceCard = false;

	[SerializeField] private string playerColor;
	[SerializeField] private string playerName;

	private bool waitPosition = false;
	private bool displayPrison = true;

	void Awake()
	{
		if(photonView.IsMine)
		{
			UI = Instantiate(PlayerUIPrefab, Vector3.zero, Quaternion.identity);
			UI.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
			playerUI = UI.GetComponent<PlayerUI>();

			gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
			playerTurnScript = GameObject.Find("Canvas").transform.Find("PlayerTurn").GetComponent<PlayerTurn>();
		
			playerTurn = gameManager.playerNumber;

			moneyManager = GameObject.Find("Canvas").transform.Find("MoneyPanel").GetChild(playerTurn-1).GetComponent<MoneyManager>();
			cardManager = GameObject.Find("Canvas").transform.Find("DrawnCard").GetComponent<CardManager>();

			photonView.RPC("changeColor", RpcTarget.AllBuffered, PlayerPrefs.GetString("color"));
			photonView.RPC("moneyColor", RpcTarget.AllBuffered, PlayerPrefs.GetString("color"), playerTurn);
			photonView.RPC("changeName", RpcTarget.AllBuffered, PlayerPrefs.GetString("PlayerName"), playerTurn);

			for(int i = 0; i < grid.GetLength(0); i++)
	   		{
	   			switch(playerTurn)
	   			{
	   				case(1):
	   					grid[i, 0] += 0.25f;
	   					grid[i, 1] -= 0.05f; break;
	   				case(2):
	   					grid[i, 0] += 0.05f;
	   					grid[i, 1] += 0.25f; break;
	   				case(3):
	   					grid[i, 0] -= 0.25f; 
	   					grid[i, 1] += 0.05f; break;
	   				case(4):
	   					grid[i, 0] -= 0.05f;
	   					grid[i, 1] -= 0.25f; break;
	   			}
	   		}

	   		transform.position = new Vector3(grid[position, 0], 0.25f, grid[position, 1]);
	   		newPosition = transform.position;

	   		dices[0] = GameObject.Find("Dices").transform.Find("Dice1").GetComponent<RollDice>();
	   		dices[1] = GameObject.Find("Dices").transform.Find("Dice2").GetComponent<RollDice>();

	   		for(int i = 0; i <= 5; i++) 
	   			playerUI.buyOptions.transform.GetChild(i).gameObject.SetActive(false);
	   		playerUI.sellOptions.SetActive(false);

	   		nfi = new NumberFormatInfo {NumberGroupSeparator = " ", NumberDecimalDigits = 0};

	   		photonView.RPC("changeNumberFreeExit", RpcTarget.AllBuffered, playerTurn, freePrisonExit);
	   		
	   		StartCoroutine(PlayerLoop());

	   	}
	}

	[PunRPC]
	public void changeColor(string colorString)
	{
		playerColor = colorString;
		Color color;
		ColorUtility.TryParseHtmlString("#" + playerColor, out color);
		GetComponent<MeshRenderer>().material.color = color;
	}

	[PunRPC]
	public void moneyColor(string colorString, int player)
	{
		MoneyManager manager = GameObject.Find("Canvas").transform.Find("MoneyPanel").GetChild(player-1).GetComponent<MoneyManager>();
		Color color;
		ColorUtility.TryParseHtmlString("#" + colorString, out color);
		manager.imageColor.color = color;
	}

	[PunRPC]
	public void changeName(string nameString, int player)
	{
		playerName = nameString;
		Text usernameText = GameObject.Find("Canvas").transform.Find("MoneyPanel").GetChild(player-1).transform.Find("Username").GetComponent<Text>();
		usernameText.text = nameString;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    	if(stream.IsWriting)
    		stream.SendNext(playerColor);
    	else
    		playerColor = (string)stream.ReceiveNext();
    }

	public IEnumerator PlayerLoop()
	{
		if(photonView.IsMine)
		{
			if(playerTurnScript.playerTurn == playerTurn && gameManager.start && !waitPosition)
			{
				UI.SetActive(true);

				if(turnInPrison >= 3 && displayPrison)
				{
					playerTurnScript.Display(true, playerName + " sort de prison !");
					turnInPrison = 0;
					inPrison = false;
				}

				else if((!inPrison || tryToEscape) && buyChoice == -2 && validateSelling == 0 && !drawingCard)
				{
					playerUI.rollButton.SetActive(true);
					playerUI.prisonOptions.SetActive(false);
				}
				else if(buyChoice == -2 && validateSelling == 0 && !drawingCard && displayPrison)
				{
					playerUI.rollButton.SetActive(false);
					playerUI.prisonOptions.SetActive(true);
					if(moneyManager.currentMoney < 50000)
						playerUI.prisonYes.interactable = false;
					else
						playerUI.prisonYes.interactable = true;
					if(freePrisonExit > 0)
						playerUI.useCard.SetActive(true);
					else
						playerUI.useCard.SetActive(false);
				}
				else
				{
					playerUI.rollButton.SetActive(false);
					playerUI.prisonOptions.SetActive(false);
				}
			}
			else if(!giveMoneyOutsideTurn || (giveMoneyOutsideTurn && playerToPay == playerTurn))
				UI.SetActive(false);
			else if(moneyToPay > 0)
			{
				UI.SetActive(true);
				playerUI.rollButton.SetActive(false);
				playerUI.prisonOptions.SetActive(false);
				if(moneyManager.currentMoney >= moneyToPay)
		    	{
		    		moneyManager.Spend(true, -moneyToPay);
		    		photonView.RPC("giveMoney", RpcTarget.AllBuffered, playerToPay, moneyToPay);
		    		moneyToPay = -100000000;
		    	}
		    	else
		    	{
		    		yield return StartCoroutine(Bankrupt(moneyToPay - moneyManager.currentMoney));
		    		if(playerTurnScript.isEliminated[playerTurn-1])
		    			photonView.RPC("giveMoney", RpcTarget.AllBuffered, playerToPay, playerTurnScript.moneyWhenEliminated[playerTurn-1]);
		    		else
		    			photonView.RPC("giveMoney", RpcTarget.AllBuffered, playerToPay, moneyToPay);
		    	}
		    	yield return new WaitForSeconds(0.5f);
		    	giveMoneyOutsideTurn = false;
			}

			StartCoroutine(Move());

			bool playersLeft = false;
		   	for(int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
		   	{
		   		if(i != playerTurn - 1)
		   			playersLeft = playersLeft || !playerTurnScript.isEliminated[i];
		   	}
		   	if((!playerTurnScript.isEliminated[playerTurn-1] && playersLeft) || PhotonNetwork.CurrentRoom.MaxPlayers == 1) //Pour pouvoir tester à 1 joueur
		   	{
		   		yield return null;
		   		StartCoroutine(PlayerLoop());
		   	}
		   	else if(!playersLeft)
		   	{
		   		UI.SetActive(false);
		   		playerTurnScript.Win(true, playerName + " a gagné !!!");
		   		playerTurnScript.Display(true, "");
		   		photonView.RPC("WinFirework", RpcTarget.AllBuffered);
		   		photonView.RPC("playSound", RpcTarget.AllBuffered, "WinSound");
		   		GameObject.Find("Canvas").transform.Find("TimerText").GetComponent<GameTimer>().stop = true;
		   	}
		   	else
		   		UI.SetActive(false);
		}


		if(PhotonNetwork.IsMasterClient && gameManager.start && !moneyManagerDeactivated)
	   	{
	   		photonView.RPC("deactivateMoneyManager", RpcTarget.AllBuffered, gameManager.playerInGame);
	   		moneyManagerDeactivated = true;
	   	}
	}

	[PunRPC]
	public void WinFirework()
	{
		ParticleSystem fireworks = GameObject.Find("Particle").transform.Find("FireworksWin").GetComponent<ParticleSystem>();
		fireworks.Play();
	}

	private IEnumerator Move()
    {
    	if((actualPosition < position) && (Vector3.Distance(transform.position, newPosition) < 0.5f))
    	{
    		actualPosition = (actualPosition + 1) % grid.GetLength(0);	
    		turn = false;
    		photonView.RPC("playSound", RpcTarget.AllBuffered, "BoardNoise");
    	}
    	else if((turn && actualPosition != 0) && (Vector3.Distance(transform.position, newPosition) < 0.5f))
    	{
    		actualPosition = (actualPosition + 1) % grid.GetLength(0);
    		photonView.RPC("playSound", RpcTarget.AllBuffered, "BoardNoise");
    	}

    	newPosition = new Vector3(grid[actualPosition, 0], 0.25f, grid[actualPosition, 1]);
    	transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * speed);

    	yield return null;
    }

    public void Next()
    {
    	int random1 = dices[0].Roll();
    	int random2 = dices[1].Roll();
    	StartCoroutine(diceResult(random1, random2));
    }

    public IEnumerator diceResult(int random1, int random2)
    {
    	waitPosition = true;
    	yield return new WaitForSeconds(1.5f);
    	waitPosition = false;
    	if(!inPrison || (inPrison && random1 == random2))
    	{
    		if(random1 == random2 && !inPrison)
	    	{
	    		consecutiveDouble++;
	    		playerTurnScript.Double(true);
	    	}
	    	else if(inPrison && random1 == random2)
	    	{
				tryToEscape = false;
				playerTurnScript.Double(true);
    			playerTurnScript.Display(true, playerName + " a fait un double !\nIl peut sortir de prison");
		    	inPrison = false;
		    	turnInPrison = 0;
	    	}
	    	else
	    		consecutiveDouble = 0;


    		position += random1 + random2;

    		if(position >= grid.GetLength(0))
	    	{
	    		position -= grid.GetLength(0);
	    		turn = true;
	    		moneyManager.Spend(true, 200000);
	    		playerTurnScript.Display(true, "Case départ ! " + playerName + " reçoit 200 000€ !");
	    	}
	    	else if(consecutiveDouble >= 3)
	    	{
    			playerTurnScript.Display(true, "3 doubles d'affilée\n" + playerName + " se dirige en prison !");
	    		position = 10;
	    		inPrison = true;
	    		turn = true;
	    		turnInPrison = 0;
	    		consecutiveDouble = 0;
	    		displayPrison = false;
	    	}

	    	StartCoroutine(Case());

    	}
    	else
    	{
    		tryToEscape = false;
    		displayPrison = false;
    		turnInPrison++;
    		playerTurnScript.Next();
    		StartCoroutine(waitPrison());
       	}
    }

    public IEnumerator waitPrison()
	{
		yield return new WaitForSeconds(1f);
		displayPrison = true;
	}

    public IEnumerator Case()
    {
    	waitPosition = true;
    	while(actualPosition != position)
    		yield return null;

    	StartCoroutine(waitSynchro());

    	switch(position)
		{
			case(10): if(displayPrison) playerTurnScript.Display(true, playerName + " visite la prison !"); 
			  else photonView.RPC("playSound", RpcTarget.AllBuffered, "PrisonSound");
		  	  if(consecutiveDouble == 0) playerTurnScript.Next();
		  	  StartCoroutine(waitPrison()); break;
		    case(20): playerTurnScript.Display(true, playerName + " se repose au parking gratuit !");
		      if(consecutiveDouble == 0) playerTurnScript.Next(); break;
		    case(2):
		    case(12):
		    case(28): playerTurnScript.Display(true,playerName + " pioche une carte Caisse de Communauté");
		      drawingCard = true;
		      StartCoroutine(CommunityChest()); break;
		    case(8):
		    case(22):
		    case(32): playerTurnScript.Display(true,playerName + " pioche une carte Chance");
		      drawingCard = true;
		      StartCoroutine(Chance()); break;
		    case(18): playerTurnScript.Display(true,"Taxe foncière ! " + playerName + " doit payer 200 000€");
		      if(moneyManager.currentMoney >= 200000)
		      {
		      	moneyManager.Spend(true, -200000);
		      	if(consecutiveDouble == 0) playerTurnScript.Next();
		      }
		      else
		      	StartCoroutine(Bankrupt(200000 - moneyManager.currentMoney));
		      break;
		    case(38): playerTurnScript.Display(true,"Impots sur le revenu ! " + playerName + " doit payer 100 000€");
		      if(moneyManager.currentMoney >= 100000)
		      {
		      	moneyManager.Spend(true, -100000);
		      	if(consecutiveDouble == 0) playerTurnScript.Next(); 
		      }
		      else
		      	StartCoroutine(Bankrupt(100000 - moneyManager.currentMoney));
		      break;
		    case(30): playerTurnScript.Display(true, playerName + " se dirige en prison !");
		    		  photonView.RPC("playSound", RpcTarget.AllBuffered, "Police");
		   		      yield return new WaitForSeconds(1);
		   		      speed = 6;
			   		  position = 10;
		    		  turn = true;
		    		  displayPrison = false;
		    		  turnInPrison = 0;
		    		  consecutiveDouble = 0;
		    		  inPrison = true;
		    		  yield return StartCoroutine(Case());
		    		  speed = 3;
		    		  yield return StartCoroutine(waitSynchro());
		    		  break;
		    case(0): if(consecutiveDouble == 0) playerTurnScript.Next(); break;
		    default: StartCoroutine(Property()); break;
		}
    }

    public IEnumerator waitSynchro()
    {
    	yield return new WaitForSeconds(2f);
    	waitPosition = false;
    }

    public void prisonYes()
    {
    	moneyManager.Spend(true, -50000);
    	playerTurnScript.Display(true, playerName + " paie 50 000€ et sort de prison !");
    	inPrison = false;
    	turnInPrison = 0;
    }

    [PunRPC]
    public void deactivateMoneyManager(int playerInGame)
    {
    	for(int i = playerInGame; i < 4; i++)
    	{
	   		GameObject.Find("Canvas").transform.Find("MoneyPanel").GetChild(i).gameObject.SetActive(false);
	   		GameObject.Find("Canvas").transform.Find("FreePrisonExitPanel").GetChild(i).gameObject.SetActive(false);
    	}
    }

    public void prisonNo()
    {
    	playerTurnScript.Display(true, playerName + " essaie de s'échapper !");
    	tryToEscape = true;
    }

    public void useFreeExitCard()
    {
    	playerTurnScript.Display(true, playerName + " utilise sa carte de sortie de prison !");
    	freePrisonExit--;
    	photonView.RPC("changeNumberFreeExit", RpcTarget.AllBuffered, playerTurn, freePrisonExit);
    	inPrison = false;
    	turnInPrison = 0;
    }

    public IEnumerator Property()
    {
    	int propertyNumber = 0;
    	if(position != 5 && position != 15 && position != 25 && position != 35)
    	{
	    	for(int i = 0; i < 24; i++)
	    	{
	    		if(GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>().position == this.position)
	    		{
	    			targettedProperty = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
	    			propertyNumber = i;
	    		}
	    	}
    	}
    	else
    	{
    		for(int i = 24; i < 28; i++)
	    	{
	    		if(GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>().position == this.position)
	    		{
	    			targettedProperty = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
	    			propertyNumber = i;
	    		}
	    	}
    	}
    	playerTurnScript.Display(true, playerName + " est sur la case " + targettedProperty.fullName);
    	if(targettedProperty.monument) photonView.RPC("playSound", RpcTarget.AllBuffered, "MonumentSound");
    	else photonView.RPC("playSound", RpcTarget.AllBuffered, "PropertySound");
    	yield return new WaitForSeconds(1.5f);
    	if(targettedProperty.owner == 0)
    	//Proposition d'achat
    	{
    		playerUI.rollButton.SetActive(false);
    		buyChoice = -1;
    		for(int i = 0; i <= 5; i++)
    		{
    			playerUI.buyOptions.transform.GetChild(i).gameObject.SetActive(true);
    			if(i != 5) playerUI.buyOptions.transform.GetChild(i).Find("Text").GetComponent<Text>().text = buttonText[i] + "\n(" + (targettedProperty.basePrice+i*targettedProperty.housePrice).ToString("n", nfi) + "€)" ;
    		}
    		playerUI.buyOptions.transform.GetChild(4).gameObject.SetActive(false); //On ne peut pas acheter d'hôtel la première fois
    		if(targettedProperty.monument)
    		{
    			for(int i = 1; i <= 3; i++)
    				playerUI.buyOptions.transform.GetChild(i).gameObject.SetActive(false);
    		}
    		int cantBuyAnything = 0;
    		for(int i = 0; i <= 3; i++)
    		{
    			if(moneyManager.currentMoney < targettedProperty.basePrice + i * targettedProperty.housePrice)
    			{
    				playerUI.buyOptions.transform.GetChild(i).GetComponent<Button>().interactable = false;
    				cantBuyAnything++;
    			}
    			else
    				playerUI.buyOptions.transform.GetChild(i).GetComponent<Button>().interactable = true;
    		}
    		if(cantBuyAnything == 4)
    			buyChoice = 6;
    		

    		while(buyChoice == -1)
    			yield return null;
    		if(buyChoice < 5)
    		{
    			photonView.RPC("editProperty", RpcTarget.AllBuffered, playerTurn, buyChoice, propertyNumber, playerColor);
    			moneyManager.Spend(true, -targettedProperty.basePrice - buyChoice*targettedProperty.housePrice);
    			if(buyChoice == 0)
    				playerTurnScript.Display(true, playerName + " a acheté la case " + targettedProperty.fullName + "\npour " + (targettedProperty.basePrice + buyChoice*targettedProperty.housePrice).ToString("n", nfi) + "€");
    			else if(buyChoice == 1)
    				playerTurnScript.Display(true, playerName + " a acheté la case " + targettedProperty.fullName + " avec 1 maison\npour " + (targettedProperty.basePrice + buyChoice*targettedProperty.housePrice).ToString("n", nfi) + "€");
    			else
    				playerTurnScript.Display(true, playerName + " a acheté la case " + targettedProperty.fullName + " avec " + buyChoice.ToString() + " maisons\npour " + (targettedProperty.basePrice + buyChoice*targettedProperty.housePrice).ToString("n", nfi) + "€");
    		}		

    		else if(buyChoice == 5)
    			playerTurnScript.Display(true, playerName + " n'a pas acheté la case " + targettedProperty.fullName);
    		else
    			playerTurnScript.Display(true, playerName + " n'a pas assez d'argent pour acheter la case " + targettedProperty.fullName);
	    	
	    	for(int i = 0; i <= 5; i++)
    			playerUI.buyOptions.transform.GetChild(i).gameObject.SetActive(false);

    		yield return new WaitForSeconds(1.5f);
    		if(consecutiveDouble == 0) playerTurnScript.Next(); 
    	}
    	else if(targettedProperty.owner != playerTurn)
    	//Case appartenant à un autre joueur
    	{
    		if(toMonumentFromChanceCard) targettedProperty.currentPrice *= 2;
    		if(moneyManager.currentMoney >= targettedProperty.currentPrice)
    		{
	    		moneyManager.Spend(true, -targettedProperty.currentPrice);
	    		photonView.RPC("giveMoney", RpcTarget.AllBuffered, targettedProperty.owner, targettedProperty.currentPrice);
    			playerTurnScript.Display(true, playerName + " doit payer " + targettedProperty.currentPrice.ToString("n",nfi) + "€");
    			if(consecutiveDouble == 0) playerTurnScript.Next(); 
    		}
	    	else
	    	{
	    		yield return StartCoroutine(Bankrupt(targettedProperty.currentPrice - moneyManager.currentMoney));
	    		if(playerTurnScript.isEliminated[playerTurn-1]) 
	    			photonView.RPC("giveMoney", RpcTarget.AllBuffered, targettedProperty.owner, playerTurnScript.moneyWhenEliminated[playerTurn-1]);
	    		else
	    			photonView.RPC("giveMoney", RpcTarget.AllBuffered, targettedProperty.owner, targettedProperty.currentPrice);
	    	}
	    	if(toMonumentFromChanceCard) targettedProperty.currentPrice /= 2;
    	}
    	else if(!targettedProperty.monument)
    	//La propriété nous appartient déjà (et ce n'est pas un monument)
    	{
    		playerUI.rollButton.SetActive(false);
    		buyChoice = -1;
    		for(int i = targettedProperty.level + 1; i <= 5; i++)
    		{
    			playerUI.buyOptions.transform.GetChild(i).gameObject.SetActive(true);
    			if(i != 5) playerUI.buyOptions.transform.GetChild(i).Find("Text").GetComponent<Text>().text = buttonText[i] + "\n(" + ((i-targettedProperty.level)*targettedProperty.housePrice).ToString("n", nfi) + "€)" ;
    		}

    		int cantBuyAnything = 0;
    		for(int i = targettedProperty.level + 1; i <= 4; i++)
    		{
    			if(moneyManager.currentMoney < (i-targettedProperty.level)*targettedProperty.housePrice)
    			{
    				playerUI.buyOptions.transform.GetChild(i).GetComponent<Button>().interactable = false;
    				cantBuyAnything++;
    			}
    			else
    				playerUI.buyOptions.transform.GetChild(i).GetComponent<Button>().interactable = true;
    		}
    		if(targettedProperty.level == 4)
    			buyChoice = 6;
    		else if(cantBuyAnything == 4 - targettedProperty.level)
    			buyChoice = 5;

    		while(buyChoice == -1)
    			yield return null;

    		if(buyChoice < 5)
    		{
    			moneyManager.Spend(true, -(buyChoice-targettedProperty.level)*targettedProperty.housePrice);
    			if(buyChoice == 1)
    				playerTurnScript.Display(true, targettedProperty.fullName + " possède maintenant 1 maison\npour " + ((buyChoice-targettedProperty.level)*targettedProperty.housePrice).ToString("n",nfi) + "€");
    			else if(buyChoice == 2 ||buyChoice == 3)
    				playerTurnScript.Display(true, targettedProperty.fullName + " possède maintenant " + buyChoice.ToString() + " maisons\npour " + ((buyChoice-targettedProperty.level)*targettedProperty.housePrice).ToString("n", nfi) + "€");
    			else if(buyChoice == 4)
    				playerTurnScript.Display(true, targettedProperty.fullName + " possède maintenant un hôtel\npour " + ((buyChoice-targettedProperty.level)*targettedProperty.housePrice).ToString("n", nfi) + "€");
    			photonView.RPC("editProperty", RpcTarget.AllBuffered, playerTurn, buyChoice, propertyNumber, playerColor);
    		}		
    		else if(buyChoice == 5)
    			playerTurnScript.Display(true, playerName + " n'a pas amélioré la case " + targettedProperty.fullName);
    		else
    			playerTurnScript.Display(true, playerName + " ne peut pas construire davantage sur la case " + targettedProperty.fullName);
	    	
	    	for(int i = 0; i <= 5; i++)
    			playerUI.buyOptions.transform.GetChild(i).gameObject.SetActive(false);

    		if(consecutiveDouble == 0) playerTurnScript.Next(); 
    	}
    	else if(consecutiveDouble == 0) 
    		playerTurnScript.Next();

    	yield return new WaitForSeconds(0.5f);
    	buyChoice = -2;
    	if(toMonumentFromChanceCard) toMonumentFromChanceCard = false;
    }

    [PunRPC]
    public void editProperty(int newOwner, int newLevel, int propertyNumber, string colorString)
    {
    	Color _color;
    	ColorUtility.TryParseHtmlString("#" + colorString, out _color);
    	PropertyManager property = GameObject.Find("Properties").transform.GetChild(propertyNumber).GetComponent<PropertyManager>();
    	property.owner = newOwner;
    	property.level = newLevel;
    	property.color = _color;

    	if(newOwner == 0)
    		property.Burn();
    }

    public void buyButton() { buyChoice = 0; }

    public void oneHouseButton() { buyChoice = 1; }

    public void twoHouseButton() { buyChoice = 2; }

	public void threeHouseButton() { buyChoice = 3; }

	public void hotelButton() { buyChoice = 4; }

	public void letButton() { buyChoice = 5; }	

	public IEnumerator Bankrupt(int money)
	{
		playerTurnScript.Display(true, playerName + " est en faillite\nIl lui manque " + money.ToString("n", nfi) + "€");
		validateSelling = -1; //Pour ne pas afficher le reste de l'UI
		yield return new WaitForSeconds(1.5f);

		int maxSellingValue = 0;

		for(int i = 0; i < 28; i++)
	    {
	    	PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
	    	if(property.owner == playerTurn)
	    		maxSellingValue += (property.basePrice + property.housePrice * property.level) / 2;
	    }

	    if(maxSellingValue < money)
	    //Le joueur n'a plus assez d'argent, il est éliminé
	    {
	    	int playerLeft = 0;
	    	for(int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
	    	{
	    		if(!playerTurnScript.isEliminated[i] && i != playerTurn-1)
	    			playerLeft++;
	    	}
	    	if(playerLeft != 1)
	    	{
	    		playerTurnScript.Display(true, playerName + " est éliminé !");
	    		photonView.RPC("playSound", RpcTarget.AllBuffered, "EliminationSound");
	    	}	   
	    	
	    	GameObject explosion = Instantiate(explosionPrefab, new Vector3(transform.position.x, 0.27f, transform.position.z), transform.rotation) as GameObject;
            explosion.GetComponent<ParticleSystem>().Play();
	    	//Indiquer qu'il est éliminé à PlayerTurnScript et sauvegarder l'argent qu'il avait lors de son élimination
	    	for(int i = 0; i < 28; i++)
		    {
		    	PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
		    	if(property.owner == playerTurn)
		    		photonView.RPC("editProperty", RpcTarget.AllBuffered, 0, 0, i, ColorUtility.ToHtmlStringRGB(Color.black));
		    }
		    playerTurnScript.Eliminated(true, playerTurn-1, moneyManager.currentMoney + maxSellingValue);
		    photonView.RPC("removePawn", RpcTarget.AllBuffered, photonView.ViewID);
		    moneyManager.Spend(true, -moneyManager.currentMoney);
		    UI.SetActive(false);
		    consecutiveDouble = 0;
		    giveMoneyOutsideTurn = false;
	    }
	    else
	    //Le joueur peut avoir assez d'argent en hypothéquant ses propriétés
	    {
	    	playerTurnScript.Display(true, playerName + " peut payer sa dette (" + money.ToString("n", nfi) + "€)\nen vendant ses propriétés");
	    	playerUI.sellOptions.SetActive(true);

	    	int numberOfProperties = 0;
    		for(int i = 0; i < 28; i++)
	    	{
	    		PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
	    		if(property.owner == playerTurn)
	    			numberOfProperties++;
	    	}

	    	RectTransform content = playerUI.sellOptions.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();
	    	content.sizeDelta = new Vector2(0, 100*numberOfProperties + 50);
	    	RectTransform sellTextTransform = content.Find("SellText").GetComponent<Text>().GetComponent<RectTransform>();
	    	sellTextTransform.anchoredPosition = new Vector3(0f, (100*numberOfProperties + 50)/2 - 40, 0f);

	    	GameObject[] toggleButton = new GameObject[numberOfProperties];
	    	Vector3 formerPosition;
	    	//Créer les toggles
	    	toggleButton[0] = Instantiate(playerUI.togglePrefab, new Vector3(0f, sellTextTransform.anchoredPosition.y - 75f, 0f), Quaternion.identity) as GameObject;
	    	formerPosition = toggleButton[0].GetComponent<RectTransform>().position;
	    	toggleButton[0].GetComponent<RectTransform>().SetParent(content);
	    	toggleButton[0].GetComponent<RectTransform>().anchoredPosition = formerPosition;
	    	toggleButton[0].GetComponent<RectTransform>().localScale = Vector3.one;
	    	toggleButton[0].GetComponent<RectTransform>().sizeDelta = new Vector2(650f, 50f);

	    	for(int i = 1; i < numberOfProperties; i++)
	    	{
	    		toggleButton[i] = Instantiate(playerUI.togglePrefab, new Vector3(0f, sellTextTransform.anchoredPosition.y - 75f - (50f*i), 0f), Quaternion.identity) as GameObject;
	    		formerPosition = toggleButton[i].GetComponent<RectTransform>().position;
		    	toggleButton[i].GetComponent<RectTransform>().SetParent(content);
		    	toggleButton[i].GetComponent<RectTransform>().anchoredPosition = formerPosition;
		    	toggleButton[i].GetComponent<RectTransform>().localScale = Vector3.one;
		    	toggleButton[i].GetComponent<RectTransform>().sizeDelta = new Vector2(650f, 50f);
	    	}
	    	int index = 0;
	    	for(int i = 0; i < 28; i++)
	    	{
	    		PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
	    		if(property.owner == playerTurn)
	    			toggleButton[index++].transform.Find("Label").GetComponent<Text>().text = property.fullName + " (" + ((property.basePrice + property.housePrice * property.level) / 2).ToString("n", nfi) + "€)";
	    	}
    		Button validateButton = playerUI.sellOptions.transform.Find("ValidateButton").GetComponent<Button>();
    		validateButton.interactable = false;
    		Text validateText = playerUI.sellOptions.transform.Find("ValidateButton").GetChild(0).GetComponent<Text>();

    		soldProperties = new bool[numberOfProperties];
    		int indexProperty;

    		while(validateSelling == -1)
    		{
    			somme = 0;
    			for(int i = 0; i < numberOfProperties; i++)
    			{
    				if(toggleButton[i].GetComponent<Toggle>().isOn)
    					soldProperties[i] = true;
    				else
    					soldProperties[i] = false;
    			}

    			indexProperty = 0;
    			for(int i = 0; i < 28; i++)
    			{
    				PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
    				if(property.owner == playerTurn)
    				{
    					if(soldProperties[indexProperty++])
    						somme += (property.basePrice + property.housePrice * property.level) / 2;
    				}
    			}

    			if(somme >= money)
    			{
    				validateButton.interactable = true;
    				validateText.text = "Valider";
    			}
    			else
    			{
    				validateButton.interactable = false;
    				validateText.text = "Il manque " + (money-somme).ToString("n", nfi) + "€";
    			}
    			yield return null;
    		}
    		playerUI.sellOptions.SetActive(false);

    		indexProperty = 0;
    		for(int i = 0; i < 28; i++)
    		{
    			PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
    			if(property.owner == playerTurn)
    			{
    				if(soldProperties[indexProperty++])
    				{
    					photonView.RPC("editProperty", RpcTarget.AllBuffered, 0, 0, i, ColorUtility.ToHtmlStringRGB(Color.black));
    				}
    			}
    		}
    		moneyManager.Spend(true, -money-moneyManager.currentMoney+somme);
    		playerTurnScript.Display(true, playerName + " a vendu des propriétés pour " + somme.ToString("n", nfi) + "€");

    		for(int i = playerUI.sellOptions.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>().childCount - 1; i > 0; i--)
			{
				Transform child = playerUI.sellOptions.transform.Find("Viewport").Find("Content").transform.GetChild(i);
				child.SetParent(null);
			}
	    }
	    StartCoroutine(waitSelling());
	    if(consecutiveDouble == 0 && !giveMoneyOutsideTurn) playerTurnScript.Next(); 
	    else if(giveMoneyOutsideTurn) giveMoneyOutsideTurn = false;
	}

	public IEnumerator waitSelling()
	{
		yield return new WaitForSeconds(0.75f);
		validateSelling = 0;
	}

	public void validateSell() { validateSelling = 1; }

	[PunRPC]
	public void removePawn(int playerID)
	{
		PhotonView.Find(playerID).GetComponent<MeshRenderer>().enabled = false;
	}

	public IEnumerator CommunityChest()
	{
		GameObject moneyPanel = GameObject.Find("Canvas").transform.Find("MoneyPanel").gameObject;
		int[] currentMoneyOfAllPlayers = {0, 0, 0, 0};
		bool allMoneyGot;
		bool nextTurn = true;

		cardManager.drawNewNumber(false);
		yield return new WaitForSeconds(0.75f); //wait for the RPC to be sent
		cardManager.DrawCommunityChestCard(true);
		yield return StartCoroutine(cardManager.disableCardImage());
		switch(cardManager.randomNumber)
		{
			case(0): moneyManager.Spend(true, 200000); 
                     playerTurnScript.Display(true,playerName + " passe par la case Départ et reçoit 200 000€");
                     position = 0;
                     turn = true; break;
			case(1): moneyManager.Spend(true, 200000); 
                     playerTurnScript.Display(true, playerName + " reçoit 200 000€"); break;
			case(2): playerTurnScript.Display(true, playerName + " doit payer 50 000€"); 
					 if(moneyManager.currentMoney >= 50000)
					 	moneyManager.Spend(true, -50000);
					 else
					 {
					 	yield return StartCoroutine(Bankrupt(50000 - moneyManager.currentMoney));
					 	nextTurn = false;
					 }
					 break;
			case(3): moneyManager.Spend(true, 50000); 
                     playerTurnScript.Display(true, playerName + " reçoit 50 000€"); break;
			case(4): playerTurnScript.Display(true, playerName + " reçoit une carte de sortie de prison gratuite");
					 freePrisonExit++;
					 photonView.RPC("changeNumberFreeExit", RpcTarget.AllBuffered, playerTurn, freePrisonExit); break;
			case(5): playerTurnScript.Display(true, playerName + " se dirige en prison !");
				     position = 10;
	    			 inPrison = true;
	    			 photonView.RPC("playSound", RpcTarget.AllBuffered, "PrisonSound");
	    			 turn = true;
	    			 turnInPrison = 0;
	    			 consecutiveDouble = 0; break;
			case(6): 
				     for(int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
				     	currentMoneyOfAllPlayers[i] = moneyPanel.transform.GetChild(i).GetComponent<MoneyManager>().currentMoney;
				     photonView.RPC("askForMoney", RpcTarget.OthersBuffered, playerTurn, 50000);
				     do
				     {
				     	allMoneyGot = true;
				     	for(int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
				     	{
				     		if(i != playerTurn-1 && !playerTurnScript.isEliminated[i])
				     			allMoneyGot = allMoneyGot && ((currentMoneyOfAllPlayers[i] != moneyPanel.transform.GetChild(i).GetComponent<MoneyManager>().currentMoney) || playerTurnScript.isEliminated[i]);
				     	}
				     	yield return new WaitForSeconds(1);
				     } while(!allMoneyGot); break;
			case(7): moneyManager.Spend(true, 100000); 
                     playerTurnScript.Display(true, playerName + " reçoit 100 000€"); break;
			case(8): moneyManager.Spend(true, 20000); 
                     playerTurnScript.Display(true, playerName + " reçoit 20 000€"); break;
			case(9): for(int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
				     	currentMoneyOfAllPlayers[i] = moneyPanel.transform.GetChild(i).GetComponent<MoneyManager>().currentMoney;
				     photonView.RPC("askForMoney", RpcTarget.OthersBuffered, playerTurn, 10000);
				     do
				     {
				     	allMoneyGot = true;
				     	for(int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
				     	{
				     		if(i != playerTurn-1 && !playerTurnScript.isEliminated[i])
				     			allMoneyGot = allMoneyGot && ((currentMoneyOfAllPlayers[i] != moneyPanel.transform.GetChild(i).GetComponent<MoneyManager>().currentMoney) || playerTurnScript.isEliminated[i]);
				     	}
				     	yield return new WaitForSeconds(1);
				     } while(!allMoneyGot); break;
			case(10):moneyManager.Spend(true, 100000); 
                     playerTurnScript.Display(true, playerName + " reçoit 100 000€"); break;
			case(11):playerTurnScript.Display(true, playerName + " doit payer 50 000€"); 
					 if(moneyManager.currentMoney >= 50000)
					 	moneyManager.Spend(true, -50000);
					 else
					 {
					 	yield return StartCoroutine(Bankrupt(50000 - moneyManager.currentMoney));
					 	nextTurn = false;
					 }
					  break;
			case(12):playerTurnScript.Display(true, playerName + " doit payer 50 000€"); 
					 if(moneyManager.currentMoney >= 50000)
					 	moneyManager.Spend(true, -50000);
					 else
					 {
					 	yield return StartCoroutine(Bankrupt(50000 - moneyManager.currentMoney));
					 	nextTurn = false;
					 }
					 break;
			case(13):moneyManager.Spend(true, 25000); 
                     playerTurnScript.Display(true, playerName + " reçoit 25 000€"); break;
			case(14):int toPay = 0;
					 for(int i = 0; i < 28; i++)
				     {
				    	  PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
				    	  if(property.owner == playerTurn)
				    	  {
				    	  	if(property.level == 4) toPay += 115000;
				    	  	else toPay += property.level * 40000;
				    	  }
				     }
				     playerTurnScript.Display(true, playerName + " doit payer " + toPay.ToString("n", nfi) + "€");
				     if(moneyManager.currentMoney >= toPay)
					 	moneyManager.Spend(true, -toPay);
					 else
					 {
					 	yield return StartCoroutine(Bankrupt(toPay - moneyManager.currentMoney));
					 	nextTurn = false;
					 }	
					 break;
			case(15):moneyManager.Spend(true, 10000); 
                     playerTurnScript.Display(true, playerName + " reçoit 10 000€"); break;
			case(16):moneyManager.Spend(true, 100000); 
                     playerTurnScript.Display(true, playerName + " reçoit 100 000€"); break;
		}
		StartCoroutine(waitCard());
		if(consecutiveDouble == 0 && nextTurn) playerTurnScript.Next();
	}

	public IEnumerator Chance()
	{
		bool nextTurn = true;
		cardManager.drawNewNumber(true);
		yield return new WaitForSeconds(0.5f); //wait for the RPC to be sent
		cardManager.DrawChanceCard(true);
		yield return StartCoroutine(cardManager.disableCardImage());
		switch(cardManager.randomNumber)
		{
			case(0): moneyManager.Spend(true, 50000); 
                     playerTurnScript.Display(true, playerName + " reçoit 50 000€"); break;
			case(1): if(position > 24)
					 {
					 	playerTurnScript.Display(true, playerName + " passe par la case Départ et reçoit 200 000€");
						moneyManager.Spend(true, 200000);
					 }
					 position = 24;
					 turn = true;
					 yield return StartCoroutine(Case());
					 nextTurn = false; break;
			case(2): moneyManager.Spend(true, 150000); 
                     playerTurnScript.Display(true, playerName + " reçoit 150 000€"); break;
			case(3): moneyManager.Spend(true, 200000); 
                     playerTurnScript.Display(true,playerName + " passe par la case Départ et reçoit 200 000€");
                     position = 0;
                     turn = true; break;
			case(4): if(position > 5)
					 {
					 	playerTurnScript.Display(true, playerName + " passe par la case Départ et reçoit 200 000€");
						moneyManager.Spend(true, 200000);
					 }
					 position = 5;
					 turn = true;
					 yield return StartCoroutine(Case());
					 nextTurn = false; break;
			case(5): if(position > 11)
					 {
					 	playerTurnScript.Display(true, playerName + " passe par la case Départ et reçoit 200 000€");
						moneyManager.Spend(true, 200000);
					 }
					 position = 11;
					 turn = true;
					 yield return StartCoroutine(Case());
					 nextTurn = false; break;
			case(6): moneyManager.Spend(true, 100000); 
                     playerTurnScript.Display(true, playerName + " reçoit 100 000€"); break;
			case(7): int playerToPay = 0;
                     for(int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
                     {
                        if(i != playerTurn-1 && !playerTurnScript.isEliminated[i])
                            playerToPay++;
                     }
                     playerTurnScript.Display(true, playerName + " doit payer " + (playerToPay*50000).ToString("n", nfi) + "€");
                     if(moneyManager.currentMoney >= playerToPay * 50000)
					 	moneyManager.Spend(true, -playerToPay*50000);
					 else
					 {
					 	yield return StartCoroutine(Bankrupt(playerToPay*50000 - moneyManager.currentMoney));
					 	nextTurn = false;
					 }
					 if(!playerTurnScript.isEliminated[playerTurn-1])
					 {
					 	for(int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
	                     {
	                        if(i != playerTurn-1 && !playerTurnScript.isEliminated[i])
	                            photonView.RPC("giveMoney", RpcTarget.AllBuffered, i+1, 50000);
	                     }
					 }
					 else
					 {
					 	for(int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
	                     {
	                        if(i != playerTurn-1 && !playerTurnScript.isEliminated[i])
	                            photonView.RPC("giveMoney", RpcTarget.AllBuffered, i+1, playerTurnScript.moneyWhenEliminated[playerTurn-1] / 3);
	                     }
					 }
					 break;
			case(8): playerTurnScript.Display(true, playerName + " se dirige en prison !");
				     position = 10;
	    			 inPrison = true;
	    			 photonView.RPC("playSound", RpcTarget.AllBuffered, "PrisonSound");
	    			 turn = true;
	    			 turnInPrison = 0;
	    			 consecutiveDouble = 0; break;
			case(9)://Pareil que le cas 10
			case(10):turn = true;
                     if(position == 8) position = 15;
                     else if(position == 22) position = 25;
                     else if(position == 32) position = 35;
                     toMonumentFromChanceCard = true;
                     yield return StartCoroutine(Case());
                     nextTurn = false; break;
			case(11):turn = true;
					 position -= 3;
					 yield return StartCoroutine(Case());
					 nextTurn = false; break;
			case(12):position = 39;
                     turn = true;
                     yield return StartCoroutine(Case());
                     nextTurn = false; break;
			case(13):int toPay = 0;
					 for(int i = 0; i < 28; i++)
				     {
				    	  PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
				    	  if(property.owner == playerTurn)
				    	  	toPay += property.level * 25000;
				     }
				     playerTurnScript.Display(true, playerName + " doit payer " + toPay.ToString("n", nfi) + "€");
				     if(moneyManager.currentMoney >= toPay)
					 	moneyManager.Spend(true, -toPay);
					 else
					 {
					 	yield return StartCoroutine(Bankrupt(toPay - moneyManager.currentMoney));
					 	nextTurn = false;
					 }
					 break;
			case(14):playerTurnScript.Display(true, playerName + " reçoit une carte de sortie de prison gratuite");
					 freePrisonExit++;
					 photonView.RPC("changeNumberFreeExit", RpcTarget.AllBuffered, playerTurn, freePrisonExit); break;
			case(15):playerTurnScript.Display(true, playerName + " doit payer 15 000€");
				     if(moneyManager.currentMoney >= 15000)
					 	moneyManager.Spend(true, -15000);
					 else
					 {
					 	yield return StartCoroutine(Bankrupt(15000 - moneyManager.currentMoney));
					 	nextTurn = false;
					 }
					 break;
		}
		StartCoroutine(waitCard());
		if(consecutiveDouble == 0 && nextTurn) playerTurnScript.Next();
	}

	public IEnumerator waitCard()
	{
		yield return new WaitForSeconds(0.75f);
		drawingCard = false;
	}

	[PunRPC]
    public void giveMoney(int owner, int price)
    {
    	if(PhotonNetwork.IsMasterClient)
    	{
    		MoneyManager targetMoneyManager = GameObject.Find("Canvas").transform.Find("MoneyPanel").transform.GetChild(owner - 1).GetComponent<MoneyManager>();
    		targetMoneyManager.Spend(true, price);
    	}    	
    }

    [PunRPC]
    public void askForMoney(int myNumber, int moneyAsked)
    {
    	giveMoneyOutsideTurn = true;
    	playerToPay = myNumber;
    	moneyToPay = moneyAsked;
    }

    [PunRPC]
    public void playSound(string sound)
    {
    	GameObject.Find("Audio").transform.Find(sound).GetComponent<AudioSource>().Play();
    }

    [PunRPC]
    public void changeNumberFreeExit(int player, int exit)
    {
    	GameObject playerObject = GameObject.Find("Canvas").transform.Find("FreePrisonExitPanel").transform.GetChild(player - 1).gameObject;

    	if(exit == 0)
    		playerObject.SetActive(false);
    	else
    	{
    		playerObject.SetActive(true);
    		playerObject.transform.GetChild(1).GetComponent<Text>().text = "<b>x"+exit+"</b>";
    	}
    }
}