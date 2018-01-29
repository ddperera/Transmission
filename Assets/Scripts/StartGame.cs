using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MEC;

public class StartGame : MonoBehaviour {

	public AudioClip startSound;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("ShiftUp"))
		{
			GetComponent<AudioSource>().PlayOneShot(startSound, 4f);
			Timing.CallDelayed(1f, delegate { SceneManager.LoadScene(1); });
		}
	}
}
