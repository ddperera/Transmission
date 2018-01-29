using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGame : MonoBehaviour {

	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("ShiftDown"))
		{
			Application.Quit();
		}
	}
}
