using KinematicCharacterController;
using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransmissionCharacterController : BaseCharacterController {

	[Header("Stable Movement")]
	public float[] moveSpeeds;	//movement speed placed along bike's forward vector to determine desired velocity (m/s)
	public float[] turnRates;	//how quickly input rotates the bike (deg/s)
	public float[] velocityTurnAdjustmentRates;		//how quickly velocity slerps to desired velocity (fraction/frame @30fps)
	public float[] velocityMagnitudeAdjustmentRates;    //how quickly velocity magnitude lerps towards desired velocity (fraction/frame @30fps)
	//public float skidDotProductDivisor = 1.25f;	//divides the dot product to push the skid ranges more fluidly into 0-1. Be very careful this will have a strong effect!
	public float skidMultiplierExponent = 6f;	//what power to raise the dot product of target velocity and velocity, higher will cause a greater skidding effect
	public float minSkidMultiplier= .25f;	//minimum target velocity multiplier caused by skidding


	[Header("Air Movement")]
	public float airDrag = 0.1f; // Air drag
	private const float GRAVITY_ACCELERATION = 9.81f;


	[Header("Camera")]
	public Transform cameraSwivelTarget;
	private Vector3 m_swivelOrigin;
	public TransmissionCameraSwivel cameraSwivelFollower;
	public float skidEffectsMultiplierExponent = 3f;
	public float skidSwivelMovementFraction = 2f;
	public float skidCameraHeightImpact = .75f;
	public float skidCameraDistanceImpact = 1.25f;


	[Header("Mesh Visuals")]
	public GameObject visualsObj;
	private RotateWheels m_rotateWheelsRef;
	public float maxLeanRotation;

	[Header("VFX")]
	public ParticleSystem[] skidSparksParticles;
	public float skidSparksThreshold = .75f;
	public float skidSparksMaxRateMultiplier = 1f;
	public float skidSparksMaxStartSpeedMultiplier = 1f;
	public TrailRenderer[] skidTrails;
	public float skidTrailsThreshold = .85f;
	public float skidTrailsMaxColorAlpha;

	[Header("UI")]
	public Text shiftText;
	public Text speedText;

	//internal
	private float m_turnInput = 0f;
	private Vector3 m_desiredRot;
	private Vector3 m_moveInput = Vector3.zero;

	// Use this for initialization
	void Start()
	{
		m_curShiftIdx = 0;
		cameraSwivelFollower.SetSwivelTarget(cameraSwivelTarget, this);
		m_swivelOrigin = cameraSwivelTarget.localPosition;
		m_rotateWheelsRef = visualsObj.GetComponentInChildren<RotateWheels>();
		/*foreach (ParticleSystem p in skidSparksParticles)
		{
			var em = p.emission;
			em.rateOverTimeMultiplier = 0f;
		}*/
		foreach (TrailRenderer tr in skidTrails)
		{
			tr.startWidth = 0f;
		}
		m_canShiftUp = true;
	}

	// Update is called once per frame
	void Update()
	{
		//input
		m_turnInput = Input.GetAxis("Horizontal");
		//m_turnInput = 1f;
		//m_turnInput = (Time.time % 10f) > 5f ? 1f : -1f;
		if (Input.GetButtonDown("ShiftUp"))
		{
			InputShiftUp();
		}
		else if (Input.GetButtonDown("ShiftDown"))
		{
			InputShiftDown();
		}
		//debug input
		if ((Debug.isDebugBuild || Application.isEditor) && Input.GetKeyDown(KeyCode.LeftBracket))
		{
			Time.timeScale = .2f;
		}
		else if ((Debug.isDebugBuild || Application.isEditor) && Input.GetKeyDown(KeyCode.RightBracket))
		{
			Time.timeScale = 1f;
		}

		//mesh visuals
		Vector3 rot = visualsObj.transform.localEulerAngles;
		rot.z = -m_turnInput * maxLeanRotation;
		visualsObj.transform.localEulerAngles = rot;
		/*foreach (ParticleSystem p in skidSparksParticles)
		{
			rot = p.gameObject.transform.localEulerAngles;
			rot.y = Mathf.Sign(-m_turnInput) * 20f;
			p.gameObject.transform.localEulerAngles = rot;
		}*/
		if (KinematicCharacterMotor.Velocity.sqrMagnitude > 0f) m_rotateWheelsRef.SetSpeed(KinematicCharacterMotor.Velocity.magnitude);
	}

	#region character controller updates
	public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
	{
		currentRotation *= Quaternion.Euler(0f, m_turnInput * GetTurnRate() * deltaTime, 0f);
		Quaternion targetRotation = Quaternion.FromToRotation(currentRotation * Vector3.up, KinematicCharacterMotor.GroundNormal) * currentRotation;
		currentRotation = Quaternion.Slerp(currentRotation, targetRotation, 1 - Mathf.Exp(-3f * deltaTime));
		//currentRotation = targetRotation;
	}

	public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
	{
		if (KinematicCharacterMotor.IsStableOnGround)
		{
			//reorient current velocity on slope
			currentVelocity = KinematicCharacterMotor.GetDirectionTangentToSurface(currentVelocity, KinematicCharacterMotor.GroundNormal) * currentVelocity.magnitude;

			//get desired forward velocity oriented on slope
			Vector3 targetVelocity = Vector3.Cross(KinematicCharacterMotor.GroundNormal, KinematicCharacterMotor.CharacterRight).normalized * -GetMoveSpeed();

			//slow target vel the more we are drifting (the higher the difference between our forward vector and vel)
			if (currentVelocity.sqrMagnitude > 0f)
			{
				float skidMultiplier = Vector3.Dot(targetVelocity.normalized, currentVelocity.normalized);
				float skidEffectsMultiplier = skidMultiplier;
				skidMultiplier = Mathf.Pow(skidMultiplier, skidMultiplierExponent);
				skidMultiplier = Mathf.Clamp(skidMultiplier, minSkidMultiplier, 1f);
				skidEffectsMultiplier = Mathf.Pow(skidEffectsMultiplier, skidEffectsMultiplierExponent);
				skidEffectsMultiplier = Mathf.Clamp01(skidEffectsMultiplier);
				float skidEffectsInvert = 1f - skidEffectsMultiplier;

				//adjust velocity
				targetVelocity *= skidMultiplier;

				//move camera swivel forward effect
				cameraSwivelTarget.localPosition = m_swivelOrigin + Vector3.forward * (1-skidMultiplier) * skidSwivelMovementFraction;

				//adjust camera distance effect
				cameraSwivelFollower.SetDesiredCameraPositionByFraction(1f - (1-skidMultiplier) * skidCameraDistanceImpact, 1f - (1-skidMultiplier) * skidCameraHeightImpact);

				//play sparks
				/*if (skidEffectsMultiplier <= skidSparksThreshold)
				{
					foreach (ParticleSystem p in skidSparksParticles)
					{
						var em = p.emission;
						em.rateOverTimeMultiplier = skidSparksMaxRateMultiplier * skidEffectsInvert * 2f;
						var main = p.main;
						main.startSpeedMultiplier = skidSparksMaxStartSpeedMultiplier * skidEffectsInvert * 2f;
					}
				}
				else
				{
					foreach (ParticleSystem p in skidSparksParticles)
					{
						var em = p.emission;
						em.rateOverTimeMultiplier = 0f;
					}
				}*/

				//show skid trails
				Color col;
				if (skidEffectsMultiplier <= skidTrailsThreshold)
				{
					foreach (TrailRenderer tr in skidTrails)
					{
						col = tr.startColor;
						col.a = skidTrailsMaxColorAlpha * skidEffectsInvert * 2f;
						tr.startColor = col;
						col = tr.endColor;
						col.a = skidTrailsMaxColorAlpha * skidEffectsInvert * 2f;
						tr.endColor = col;
					}
				}
				else
				{
					foreach (TrailRenderer tr in skidTrails)
					{
						col = tr.startColor;
						col.a = 0f;
						tr.startColor = col;
						col = tr.endColor;
						col.a = 0f;
						tr.endColor = col;
					}
				}
				//Debug.Log("val: "+skidEffectsInvert+" start color: " + skidTrails[0].startColor + " end color: " + skidTrails[0].endColor);
			}

			//move velocity towards target
			Debug.DrawRay(transform.position, currentVelocity, Color.red);
			Debug.DrawRay(transform.position, targetVelocity, Color.green);
			//currentVelocity = Vector3.RotateTowards(currentVelocity, targetVelocity, GetVelocityTurnAdjustmentRate()*deltaTime*Mathf.Deg2Rad, GetVelocityMagnitudeAdjustmentRate()*deltaTime);
			Vector3 targetVelNorm = targetVelocity.normalized;
			Vector3 curVelNorm = currentVelocity.normalized;
			curVelNorm = Vector3.Slerp(curVelNorm, targetVelNorm, GetVelocityMagnitudeAdjustmentRate() * deltaTime * 30f);
			float newMagnitude = Mathf.Lerp(currentVelocity.magnitude, targetVelocity.magnitude, GetVelocityMagnitudeAdjustmentRate() * deltaTime * 30f);
			currentVelocity = curVelNorm * newMagnitude;
			Debug.DrawRay(transform.position, currentVelocity, Color.blue);

			speedText.text = "SPEED " + Mathf.RoundToInt(currentVelocity.magnitude);
		}
		else
		{
			currentVelocity += Vector3.down * GRAVITY_ACCELERATION * deltaTime;
			currentVelocity *= (1f / (1f + (airDrag * deltaTime)));


			//turn off skid fx
			cameraSwivelTarget.localPosition = Vector3.Lerp(cameraSwivelTarget.localPosition, m_swivelOrigin, .1f*deltaTime*30f);
			cameraSwivelFollower.SetDesiredCameraPositionByFraction(1f, 1f);
			/*foreach (ParticleSystem p in skidSparksParticles)
			{
				var em = p.emission;
				em.rateOverTimeMultiplier = 0f;
			}*/
			Color col;
			foreach (TrailRenderer tr in skidTrails)
			{
				col = tr.startColor;
				col.a = 0f;
				tr.startColor = col;
				col = tr.endColor;
				col.a = 0f;
				tr.endColor = col;
			}

			speedText.text = "SPEED ???";
		}

		//DebugUtil.DrawDebugCross(transform.position, 1f, Color.green, 100f);
		/*float dist = Vector3.Distance(lastPos, transform.position);
		Debug.DrawLine(lastPos, transform.position, Color.Lerp(Color.green, Color.magenta, Mathf.Abs(dist - lastDist) / .25f), 100f);
		lastDist = dist;
		lastPos = transform.position;*/
	}
	Vector3 lastPos;
	float lastDist;

	public override void AfterCharacterUpdate(float deltaTime)
	{
		if (KinematicCharacterMotor.IsStableOnGround && !KinematicCharacterMotor.WasStableOnGround)
		{
			OnLanded();
		}
		else if (!KinematicCharacterMotor.IsStableOnGround && KinematicCharacterMotor.WasStableOnGround)
		{
			OnLeaveStableGround();
		}
	}

	public override void BeforeCharacterUpdate(float deltaTime)
	{
		
	}
	#endregion


	#region shifting
	private int m_curShiftIdx = 0;
	public int CurShift { get { return m_curShiftIdx; } }
	private bool m_canShiftUp = true;
	[Header("Shifting")]
	public float shiftUpCooldown = .75f;

	public float GetTurnRate()
	{
		return turnRates[m_curShiftIdx];
	}
	public float GetMoveSpeed()
	{
		return moveSpeeds[m_curShiftIdx];
	}
	public float GetVelocityTurnAdjustmentRate()
	{
		return velocityTurnAdjustmentRates[m_curShiftIdx];
	}
	public float GetVelocityMagnitudeAdjustmentRate()
	{
		return velocityMagnitudeAdjustmentRates[m_curShiftIdx];
	}

	private void InputShiftUp()
	{
		if (!m_canShiftUp) return;
		if (m_curShiftIdx < 4)
		{
			m_curShiftIdx++;
		}
		shiftText.text = "GEAR " + GetTextForm(m_curShiftIdx + 1);
		OnShiftUp();
		Timing.RunCoroutineSingleton(_ShiftCooldown(shiftUpCooldown), "ShiftCooldown", SingletonBehavior.Overwrite);
	}
	private void InputShiftDown()
	{
		if (m_curShiftIdx > 0)
		{
			m_curShiftIdx--;
		}
		shiftText.text = "GEAR " + GetTextForm(m_curShiftIdx + 1);
		OnShiftDown();
	}

	private void OnShiftUp()
	{

	}
	private void OnShiftDown()
	{

	}

	private IEnumerator<float> _ShiftCooldown(float duration)
	{
		m_canShiftUp = false;
		yield return Timing.WaitForSeconds(duration);
		m_canShiftUp = true;
	}
	#endregion

	#region checks
	public override bool CanBeStableOnCollider(Collider coll)
	{
		return true;
	}

	public override bool IsColliderValidForCollisions(Collider coll)
	{
		return true;
	}

	public override bool MustUpdateGrounding()
	{
		return true;
	}
	#endregion

	#region events
	public override void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, bool isStableOnHit)
	{
		
	}

	public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, bool isStableOnHit)
	{
		
	}

	public void OnLanded()
	{

	}

	public void OnLeaveStableGround()
	{

	}

	public void ForceUnground()
	{
		KinematicCharacterMotor.ForceUnground();
	}
	#endregion

	#region helpers
	private string GetTextForm(int i)
	{
		switch (i)
		{
			case 1:
				return "ONE";
			case 2:
				return "TWO";
			case 3:
				return "THREE";
			case 4:
				return "FOUR";
			case 5:
				return "FIVE";
			default:
				return "???";
		}
	}
	#endregion
}
