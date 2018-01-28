using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MEC;

public class ChangeEmissiveColor : MonoBehaviour {

	private List<Material> m_materials;

	private ParticleSystem[] m_particleSystems;
	private TrailRenderer[] m_trailRenderers;

	// Use this for initialization
	void Start () {
		m_materials = new List<Material>();
		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		{
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
	}

	private void Update()
	{
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

	public void ChangeColor(Color color, float emissionBrightness, float duration, Ease easing = Ease.InQuart)
	{
		foreach (Material m in m_materials)
		{
			m.DOColor(color*emissionBrightness, "_EmissionColor", duration).SetEase(easing);
			m.DOColor(color, duration).SetEase(easing);
		}
	}
}
