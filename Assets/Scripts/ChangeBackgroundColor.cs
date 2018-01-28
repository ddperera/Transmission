using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChangeBackgroundColor : MonoBehaviour {

	private Camera m_camera;

	private void Start()
	{
		m_camera = GetComponentInChildren<Camera>();
	}

	public void ChangeColor(Color color, float duration, Ease easing = Ease.InQuart)
	{
		m_camera.DOColor(color, duration).SetEase(easing);
		m_camera.GetComponent
	}
}
