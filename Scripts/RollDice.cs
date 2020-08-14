using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class RollDice : MonoBehaviourPun
{
	private float[,] faces = {
								{0f, 0f, 0f}, {0f, 0f, -90f}, {-90f, 0f, 0f}, {90f, 0f, 0f}, {0f, 0f, 90f}, {180f, 0f, 0f}
							};
    private float[,] rotations = {{178f, -76f, -57f},{-17.762f, -95.301f, -94.124f},{223.562f, -55.916f, -48.1f},{-23.586f, -101.464f, -100.017f},{-62.2f, -122.983f, -108.87f},{143.863f, -48.867f, 5.18f}};
    private Animation animations;
    private Vector3 initialPosition;
    private AudioSource sound;

    void Start()
    {
        transform.eulerAngles = new Vector3(180f, 0f, 0f);
        initialPosition = transform.position;
        sound = GameObject.Find("Audio").transform.Find("DiceSound").GetComponent<AudioSource>();
    }

    public int Roll()
    {
        int randomNumber = Random.Range(1, 6);
        photonView.RPC("rollDice", RpcTarget.AllBuffered, randomNumber);
        return randomNumber;
    }

    [PunRPC]
    void rollDice(int random)
    {
        transform.position = initialPosition + new Vector3(0f, 10f, 0f);
        transform.eulerAngles = new Vector3(rotations[random-1, 0], rotations[random-1, 1], rotations[random-1, 2]);
        StartCoroutine(DiceSound());
    }

    public IEnumerator DiceSound()
    {
        yield return new WaitForSeconds(0.45f);
        sound.Play();
    }
}
