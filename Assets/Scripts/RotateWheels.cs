using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWheels : MonoBehaviour {

	public GameObject[] wheels;
	private const float UNSCALED_WHEEL_RADIUS = 1.775f;
	private float m_wheelRadius;

	private float m_speed;
	private float m_rotationSpeed;
	public void SetSpeed(float speed)
	{
		m_speed = speed;
	}

	private void Start()
	{
		m_wheelRadius = UNSCALED_WHEEL_RADIUS * transform.localScale.x;
	}

	void Update () {
		m_rotationSpeed = (m_speed / m_wheelRadius) * Mathf.Rad2Deg;	//rotation speed in degrees/sec
		foreach (GameObject wheel in wheels)
		{
			wheel.transform.localRotation *= Quaternion.Euler(-m_rotationSpeed * Time.deltaTime, 0f, 0f);
		}
	}
}
