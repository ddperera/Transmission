using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.PostProcessing;
using MEC;

public class ChangeBackgroundColor : MonoBehaviour {

	private Camera m_camera;

	private void Start()
	{
		m_camera = GetComponentInChildren<Camera>();
	}

	public void ChangeColor(Color color, float duration, Ease easing = Ease.InQuart)
	{
		m_camera.DOColor(color*.2f, duration).SetEase(easing);
		var behavior = GetComponent<PostProcessingBehaviour>();
		if (behavior.profile != null)
		{
			var settings = behavior.profile.vignette.settings;
			Timing.RunCoroutineSingleton(_LerpVignetteColor(settings, color, duration), "LerpVignette", SingletonBehavior.Overwrite);
		}
	}

	private IEnumerator<float> _LerpVignetteColor(VignetteModel.Settings settings, Color col, float dur)
	{
		Color startCol = settings.color;
		for (float t=0f; t<1f; t += Timing.DeltaTime/dur)
		{
			settings.color = Color.Lerp(startCol, col, t);
			yield return Timing.WaitForOneFrame;
		}
		settings.color = col;
	}
}
