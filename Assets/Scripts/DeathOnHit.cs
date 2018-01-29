using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathOnHit : MonoBehaviour {

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject.CompareTag("Player"))
		{
			Scene scene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(scene.name);
		}
	}
}
