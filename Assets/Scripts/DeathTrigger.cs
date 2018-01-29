using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathTrigger : MonoBehaviour {

	private float m_lastGroundedTime = 0.0f;
	private KinematicCharacterController.KinematicCharacterMotor kcm;

	public float UngroundedTimeTilDeath = 5.0f;

	// Use this for initialization
	void Start () 
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		kcm = player.GetComponent<KinematicCharacterController.KinematicCharacterMotor>();
		m_lastGroundedTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(kcm.IsStableOnGround) 
		{
			m_lastGroundedTime = Time.time;
		}

		if(Time.time - m_lastGroundedTime > UngroundedTimeTilDeath) 
		{
			Scene scene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(scene.name);
		}
	}

	void OnTriggerEnter( Collider other )
	{
		if(other.CompareTag("Player")) 
		{
			GameObject player = GameObject.FindWithTag("Player");
			GameObject respawn = GameObject.FindWithTag("Respawn");
			player.transform.position = respawn.transform.position;
			player.transform.rotation = respawn.transform.rotation;
		}
	}
}
