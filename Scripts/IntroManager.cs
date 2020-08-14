using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
	public VideoClip introClip;

    void Start()
    {
        StartCoroutine(StopIntro());
    }

    public IEnumerator StopIntro()
    {
    	yield return new WaitForSeconds((float)introClip.length - 0.06f);
    	SceneManager.LoadScene(1);
    }
}
