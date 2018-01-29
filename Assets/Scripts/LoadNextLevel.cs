using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextLevel : MonoBehaviour {

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			Scene scene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(scene.buildIndex + 1);
		}
	}
}
