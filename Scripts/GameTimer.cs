using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
	public GameManager gameManager;
	public bool stop;

	private Text timerText;
	private float elapsedTime;

    void Start()
    {
    	timerText = GetComponent<Text>();
    	timerText.text = string.Empty;    
    	elapsedTime = 0f;
    	stop = false;
    }

    void Update()
    {
        if(gameManager.start && !stop)
        {
        	elapsedTime += Time.deltaTime;
        	if(elapsedTime < 3600f)
        		timerText.text = ((int)elapsedTime/60).ToString("00") + ":" + ((int)elapsedTime%60).ToString("00");
        	else
        		timerText.text = ((int)elapsedTime/3600).ToString("00") + ":" + (((int)elapsedTime%3600)/60).ToString("00") + ":" + ((int)elapsedTime%60).ToString("00");
        }
    }
}
