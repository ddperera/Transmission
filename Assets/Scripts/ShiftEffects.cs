using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShiftEffects : MonoBehaviour {

	public float lerpSpeed = 0.1f;

	private Camera m_mainCamera;
	private float targetFOV;

	// Use this for initialization
	void Start() 
	{
		m_mainCamera = Camera.main;
		targetFOV = m_mainCamera.fieldOfView;
	}
	
	// Update is called once per frame
	void Update() 
	{
		m_mainCamera.fieldOfView = Mathf.Lerp( m_mainCamera.fieldOfView, targetFOV, 30 * lerpSpeed * Time.deltaTime );
	}

	public void ShiftUp()
	{
		targetFOV += 10;
	}

	public void ShiftDown()
	{
		targetFOV -= 10;
	}
}
