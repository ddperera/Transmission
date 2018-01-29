using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MEC;

public class ChangeEmissiveColor : MonoBehaviour {

	private List<Material> m_materials;

	private ParticleSystem[] m_particleSystems;
	private TrailRenderer[] m_trailRenderers;

	private float m_curEmissionBrightness;

	// Use this for initialization
	void Start () {
		m_materials = new List<Material>();
		foreach (Renderer r in GetComponents<Renderer>())
		{
			//todo somehow only grab materials that have emission
			foreach (Material m in r.materials)
			{
				if (!m_materials.Contains(m))
				{
					m_materials.Add(m);
				}
			}
		}

		m_particleSystems = GetComponentsInChildren<ParticleSystem>();
		m_trailRenderers = GetComponentsInChildren<TrailRenderer>();

		if (m_materials.Count > 0)
		{
			m_curEmissionBrightness = m_materials[0].GetColor("_EmissionColor").r;
		}
	}

	private void Update()
	{
		if (m_materials.Count > 0)
		{
			//Color col = m_materials[0].GetColor("_EmissionColor");
			Color col = m_materials[0].color;
			foreach (ParticleSystem p in m_particleSystems)
			{
				var main = p.main;
				main.startColor = col;
			}
			foreach (TrailRenderer tr in m_trailRenderers)
			{
				tr.startColor = col;
			}
		}
	}

	public void PulseEmission(float pulseBrightnessChange, float timeToMax, Ease easing = Ease.InOutQuad)
	{
		foreach (Material m in m_materials)
		{
			Color col = m.GetColor("_EmissionColor");
			m.DOColor(col * (m_curEmissionBrightness + pulseBrightnessChange), "_EmissionColor", timeToMax).SetEase(easing);
			m.DOColor(col * m_curEmissionBrightness, "_EmissionColor", timeToMax).SetEase(easing).SetDelay(timeToMax);
		}
	}

	public void WubColor(Color color, float emissionBrightness, float durationToMax, Ease easing = Ease.InOutCubic)
	{
		foreach (Material m in m_materials)
		{
			Color orig = m.GetColor("_EmissionColor");
			m.DOColor(color * emissionBrightness, "_EmissionColor", durationToMax).SetEase(easing);
			m.DOColor(orig, "_EmissionColor", durationToMax).SetEase(easing).SetDelay(durationToMax);
		}
	}

	public void ChangeColor(Color color, float emissionBrightness, float duration, Ease easing = Ease.InQuart)
	{
		foreach (Material m in m_materials)
		{
			m.DOColor(color*emissionBrightness, "_EmissionColor", duration).SetEase(easing);
			m.DOColor(color, duration).SetEase(easing);
		}
		m_curEmissionBrightness = emissionBrightness;
	}
}
