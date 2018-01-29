using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using MEC;

public class MusicMgr : MonoBehaviour {

	public AudioSource musicIntro;
	public float introVolume;
	public AudioSource[] musicShifts;
	public float[] musicShiftVolumes;

	private int curShift = 0;

	private ChangeBackgroundColor m_changeBackgroundColorRef;
	private ChangeEmissiveColor[] m_changeEmissiveColorRefs;
	private GameObject m_cameraObj;

	// Use this for initialization
	void Start () {
		musicIntro.volume = introVolume;
		foreach (AudioSource src in musicShifts)
		{
			src.Stop();
			src.volume = 0f;
		}
		curShift = 0;
		SetupTimingArrays();

		DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
		if (m_cameraObj)
		{
			transform.position = m_cameraObj.transform.position;
		}
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		m_cameraObj = GameObject.FindWithTag("MainCamera");
		if (scene.buildIndex == 1)
		{
			m_playing = true;
			Timing.RunCoroutineSingleton(_SyncAudioEffects(), "SyncAudioEffects", SingletonBehavior.Overwrite);
			m_changeBackgroundColorRef = Camera.main.GetComponent<ChangeBackgroundColor>();
			m_changeEmissiveColorRefs = FindObjectsOfType<ChangeEmissiveColor>() as ChangeEmissiveColor[];

			musicIntro.Stop();
			foreach (AudioSource src in musicShifts)
			{
				src.Play();
				src.loop = true;
				src.volume = 0f;
			}
			curShift = 1;
			OnShift(0);
		}
		else if (scene.buildIndex == 2)
		{
			m_changeBackgroundColorRef = Camera.main.GetComponent<ChangeBackgroundColor>();
			m_changeEmissiveColorRefs = FindObjectsOfType<ChangeEmissiveColor>() as ChangeEmissiveColor[];
			if (curShift != 0) OnShift(0);
		}
		else if (scene.buildIndex == 3)
		{
			m_playing = false;
			foreach (AudioSource src in musicShifts)
			{
				src.Stop();
			}
			musicIntro.Play();
		}
	}

	public void OnShift(int newShift)
	{
		musicShifts[curShift].DOFade(0f, .5f).SetEase(Ease.InQuad);
		curShift = newShift;
		musicShifts[curShift].DOFade(musicShiftVolumes[curShift], .5f).SetEase(Ease.InQuad);
	}


	#region music effects timing
	private float freq;
	private float samplesPerBeat;
	private List<int> litePulseTimingsInSamples;
	private List<int> pulseTimingsInSamples;
	private List<int> wubTimingsInSamples;
	private List<int> colorChangeTimingsInSamples;
	private float wubPeakTime = 0.13245033f;	//3.775 bps -> ~.2649 sec/beat -> 1/2 of that up to peak time and 1/2 back down

	private void SetupTimingArrays()
	{
		freq = musicShifts[0].clip.frequency;
		samplesPerBeat = freq / 2.5166666666f; ///151 bpm -> 2.51666... beats per second

		//lite pulses
		//every beat from measure 48-52 exclusive
		//48*4 - 52*4 every beat
		litePulseTimingsInSamples = new List<int>();
		for (int i = 48 * 4; i < 52 * 4; i++)
		{
			litePulseTimingsInSamples.Add((int) (i * samplesPerBeat));
		}

		//pulses
		//every beat from 0-24 exc & every 1st and 3rd beat 24-48 exc & every beat 52-56 exc
		pulseTimingsInSamples = new List<int>();
		for (int i=0; i < 24*4; i++)
		{
			pulseTimingsInSamples.Add((int)(i * samplesPerBeat));
		}
		for (int i = 24 * 4 + 1; i < 48 * 4; i += 2)
		{
			pulseTimingsInSamples.Add((int)(i * samplesPerBeat));
		}
		for (int i = 52 * 4; i < 56 * 4; i++)
		{
			pulseTimingsInSamples.Add((int)(i * samplesPerBeat));
		}

		//wubs
		//in 6ths so gotta change to that
		float samplesPerBeatSixths = freq / 3.775f; //226.5 bpm -> 3.775 beats per second
		//starting at 24: empty measure, 3 beats measure, empty measure, 6 beats measure, repeat 4 times
		wubTimingsInSamples = new List<int>();
		for (int chunk = 0; chunk < 4; chunk++)
		{
			for (int i = 0; i < 3; i++)
			{
				wubTimingsInSamples.Add((int)((25 * 6 + i + chunk * 24) * samplesPerBeatSixths));
			}
			for (int i = 0; i<6; i++)
			{
				wubTimingsInSamples.Add((int)((27 * 6 + i + chunk * 24) * samplesPerBeatSixths));
			}
		}

		//color changes
		//every 8 beats 0-16  exc&& every 4 beats 16-24 exc && every 8 beats 48-56 exc
		colorChangeTimingsInSamples = new List<int>();
		for (int i=0; i < 16 * 4; i+=8)
		{
			colorChangeTimingsInSamples.Add((int)(i * samplesPerBeat));
		}
		for (int i=16*4; i<24*4; i += 4)
		{
			colorChangeTimingsInSamples.Add((int)(i * samplesPerBeat));
		}
		for (int i=48*4; i < 56 * 4; i += 8)
		{
			colorChangeTimingsInSamples.Add((int)(i * samplesPerBeat));
		}

	}

	private bool m_playing = true;
	private IEnumerator<float> _SyncAudioEffects()
	{
		int litePulsePlayCount, pulsePlayCount, wubPlayCount, colorChangePlayCount, nextLitePulse, nextPulse, nextWub, nextColorChange, curSample;
		litePulsePlayCount = pulsePlayCount = wubPlayCount = colorChangePlayCount = 0;
		bool litePulseFinished = false;
		bool pulseFinished = false;
		bool wubFinished = false;
		bool colorChangeFinished = false;
		while (m_playing)
		{
			nextLitePulse = litePulseTimingsInSamples[litePulsePlayCount];
			nextPulse = pulseTimingsInSamples[pulsePlayCount];
			nextWub = wubTimingsInSamples[wubPlayCount];
			nextColorChange = colorChangeTimingsInSamples[colorChangePlayCount];
			
			curSample = musicShifts[curShift].timeSamples;
			if (curSample > nextLitePulse && !litePulseFinished)
			{
				LitePulse();
				litePulsePlayCount++;
				if (litePulsePlayCount == litePulseTimingsInSamples.Count)
				{
					litePulsePlayCount = 0;
					litePulseFinished = true;
				}
			}
			if (curSample > nextPulse && !pulseFinished)
			{
				Pulse();
				pulsePlayCount++;
				if (pulsePlayCount == pulseTimingsInSamples.Count)
				{
					pulsePlayCount = 0;
					pulseFinished = true;
				}
			}
			if (curSample > nextWub && !wubFinished)
			{
				Wub();
				wubPlayCount++;
				if (wubPlayCount == wubTimingsInSamples.Count)
				{
					wubPlayCount = 0;
					wubFinished = true;
				}
			}
			if (curSample > nextColorChange && !colorChangeFinished)
			{
				ChangeColor();
				colorChangePlayCount++;
				if (colorChangePlayCount == colorChangeTimingsInSamples.Count)
				{
					colorChangePlayCount = 0;
					colorChangeFinished = true;
				}
			}

			if (litePulseFinished && pulseFinished && wubFinished && colorChangeFinished && curSample < freq * 5f)
			{
				litePulseFinished = false;
				pulseFinished = false;
				wubFinished = false;
				colorChangeFinished = false;
			}

			yield return Timing.WaitForOneFrame;
		}
	}

	private void LitePulse()
	{
		foreach (ChangeEmissiveColor em in m_changeEmissiveColorRefs)
		{
			em.PulseEmission(1f, .05f);
		}
	}

	private void Pulse()
	{
		foreach (ChangeEmissiveColor em in m_changeEmissiveColorRefs)
		{
			em.PulseEmission(1.35f, .075f);
		}
	}

	public Color[] colorOptions;
	int lastIdx = -1;
	private void Wub()
	{
		int idx = Random.Range(0, colorOptions.Length);
		while (idx == lastIdx)
		{
			idx = Random.Range(0, colorOptions.Length);
		}
		Color color = colorOptions[idx];
		foreach (ChangeEmissiveColor em in m_changeEmissiveColorRefs)
		{
			em.WubColor(color, 1.5f, wubPeakTime);
		}
	}

	private void ChangeColor()
	{
		int idx = Random.Range(0, colorOptions.Length);
		while (idx == lastIdx)
		{
			idx = Random.Range(0, colorOptions.Length);
		}
		Color color = colorOptions[idx];
		foreach (ChangeEmissiveColor em in m_changeEmissiveColorRefs)
		{
			em.ChangeColor(color, .8f, .25f);
		}
		m_changeBackgroundColorRef.ChangeColor(color, .25f);
	}

	#endregion
}
