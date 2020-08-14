using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PropertyManager : MonoBehaviourPun, IPunObservable
{
    public GameObject flagPrefab;
    public GameObject smokePrefab;
    public int position;
    public string fullName;
    public int basePrice;
    public int[] rentPrice;
    public string propertyGroup;
    public bool monument;
    public GameObject fireworksPrefab;

    [HideInInspector]public int owner;
    [HideInInspector]public int level;
    [HideInInspector]public int housePrice;
    [HideInInspector]public int currentPrice;
    public GameObject flagInstance;
    [HideInInspector]public Color color;
    [HideInInspector]public bool selected;

    private AudioSource monopoleSound;
    private AudioSource loseHouseSound;
    private int previousMonopole = 0;
    private MeshRenderer[] stars = {null, null, null, null};

    void Start()
    {
    	owner = 0; //No one own it
    	level = 0; //0 <=> no house/hotel
    	housePrice = 50000 * ((position / 10) + 1);
    	currentPrice = basePrice;
    	if(!monument) flagInstance = null;

        monopoleSound = GameObject.Find("Audio").transform.Find("MonopoleSound").GetComponent<AudioSource>();
        loseHouseSound = GameObject.Find("Audio").transform.Find("LoseHouseSound").GetComponent<AudioSource>();
    }

    void Update()
    {
    	if(owner == 0)
    	{
    		if(flagInstance != null && !monument)
    			Destroy(flagInstance);
            else if(flagInstance != null && monument)
                flagInstance.GetComponent<SpriteRenderer>().color = Color.black;
            currentPrice = basePrice;
    	}
    	else if(flagInstance == null && !monument)
    	{
    		flagInstance = Instantiate(flagPrefab, new Vector3(transform.position.x, 0.27f, transform.position.z), transform.rotation) as GameObject;

    		for(int i = 1; i <= 4; i++)
		    {
		    	stars[i-1] = flagInstance.transform.Find("Star" + i.ToString()).GetComponent<MeshRenderer>();
		    	stars[i-1].enabled = false;
		    }
    	}

    	if(flagInstance != null && !monument)
    	{
    		for(int i = 0; i < level; i++)
    			stars[i].enabled = true;
	    	if(level == 4)
	    	{
	    		for(int i = 0; i < level - 1; i++)
	    			stars[i].enabled = false;
	    	}
    	}

    	if(owner != 0)
    	{
    		if(!monument)
    		{
                flagInstance.GetComponent<MeshRenderer>().material.color = color;
    			currentPrice = rentPrice[level];
    			int monopole = 1;
    			for(int i = 0; i < 24; i++)
    			{
    				PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
    				if(!property.fullName.Equals(this.fullName) && property.propertyGroup.Equals(this.propertyGroup))
    				{
    					if(property.owner == this.owner)
    						monopole++;
    				}
    			}
    			if(monopole == 3)
                {
    				currentPrice *= 2;
                    if(previousMonopole < 3)
                    {
                        GameObject fireworks = Instantiate(fireworksPrefab, new Vector3(transform.position.x, 0.27f, transform.position.z), transform.rotation) as GameObject;
                        fireworks.GetComponent<ParticleSystem>().Play();
                        previousMonopole = 3;
                        monopoleSound.Play();
                    }
                }
                else
                    previousMonopole = monopole;
    		}
    		else
    		{
                flagInstance.GetComponent<SpriteRenderer>().color = color;
    			currentPrice = 25000;
    			for(int i = 24; i < 28; i++)
    			{
    				PropertyManager property = GameObject.Find("Properties").transform.GetChild(i).GetComponent<PropertyManager>();
    				if(!property.fullName.Equals(this.fullName) && property.owner == this.owner)
    				{
    					if(property.owner == this.owner)
    						currentPrice *= 2;
    				}
    			}
    		}
    	}
    	if(owner == 0)
    		transform.Find("Price").GetComponent<TextMesh>().text = "<b>" + (basePrice/1000).ToString() + "K€</b>";
    	else if(currentPrice < 1000000)
    		transform.Find("Price").GetComponent<TextMesh>().text = "<b>" + (currentPrice/1000).ToString() + "K€</b>";
    	else
    		transform.Find("Price").GetComponent<TextMesh>().text = "<b>" + (currentPrice/1000000).ToString() + "." + ((currentPrice%1000000)/100000).ToString() + "M€</b>";
    }	



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(currentPrice);
            stream.SendNext(owner);
            stream.SendNext(level);
        }
        else
        {
            currentPrice = (int)stream.ReceiveNext();
            owner        = (int)stream.ReceiveNext();
            level        = (int)stream.ReceiveNext();
        }
    }

    public void Burn()
    {
        GameObject smoke = Instantiate(smokePrefab, new Vector3(transform.position.x, 0.27f, transform.position.z), transform.rotation) as GameObject;
        smoke.GetComponent<ParticleSystem>().Play();
        loseHouseSound.Play();
    }
}
