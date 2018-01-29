using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTrailColor : MonoBehaviour {

	[HideInInspector]
	public float trailOpacity;

	private TrailRenderer[] m_trailRenderers;
	private Material checkMaterial;

	// Use this for initialization
	void Start () {
		m_trailRenderers = GetComponentsInChildren<TrailRenderer>();
		checkMaterial = GetComponentInChildren<MeshRenderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		Color col = checkMaterial.color;
		foreach (TrailRenderer tr in m_trailRenderers)
		{
			col.a = trailOpacity;
			tr.startColor = col;
		}
	}
}
