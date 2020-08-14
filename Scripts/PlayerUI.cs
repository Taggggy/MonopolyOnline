using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
   
   public GameObject rollButton;
   public GameObject prisonOptions;
   public GameObject buyOptions;
   public GameObject sellOptions;
   public GameObject togglePrefab;
   [HideInInspector] public Button prisonYes;
   [HideInInspector] public GameObject useCard;
   private PlayerManager target;
   private RollDice[] dices = {null, null};

   void Start()
   {
   		this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

   		rollButton.GetComponent<Button>().onClick.AddListener(target.Next);
         prisonOptions.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(target.prisonYes);
         prisonOptions.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(target.prisonNo);
         prisonOptions.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(target.useFreeExitCard);

         buyOptions.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(target.buyButton);
         buyOptions.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(target.oneHouseButton);
         buyOptions.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(target.twoHouseButton);
         buyOptions.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(target.threeHouseButton);
         buyOptions.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(target.hotelButton);
         buyOptions.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(target.letButton);
 
         prisonYes = prisonOptions.transform.Find("PrisonButtonYes").GetComponent<Button>();
         useCard = prisonOptions.transform.Find("Card").gameObject;

         sellOptions.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(target.validateSell);
  }

   void Update()
   {
	   	if(target == null)
	   	{
	   		Destroy(this.gameObject);
	   		return;
	   	}
   }

   public void SetTarget(PlayerManager _target)
   {
   		target = _target;
   }
}
