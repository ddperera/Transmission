using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransmissionCameraSwivel : MonoBehaviour {

	public float pitchLerpFraction = .1f;   //fraction to lerp pitch towards velocity (fraction/frame @30fps)
	public float cameraPositionLerpFraction = .1f;  //fraction to lerp camera distance (fraction/frame @30fps)
	public bool cameraLookAtSwivel = true;

	//refs
	private TransmissionCharacterController m_characterController;
	private Transform m_swivelTarget;
	private float m_desiredCameraDistance;
	private float m_curCameraDistance;
	private float m_startingCameraDistance;
	private float m_desiredCameraHeight;
	private float m_curCameraHeight;
	private float m_startingCameraHeight;
	private GameObject m_cameraObj;

	//debug
	Vector3 m_lastPos;
	float m_lastVel;

	private void Start()
	{
		m_cameraObj = GetComponentInChildren<Camera>().gameObject;
		m_desiredCameraDistance = m_cameraObj.transform.localPosition.z;
		m_curCameraDistance = m_desiredCameraDistance;
		m_startingCameraDistance = m_curCameraDistance;
		m_desiredCameraHeight = m_cameraObj.transform.localPosition.y;
		m_curCameraHeight = m_desiredCameraHeight;
		m_startingCameraHeight = m_curCameraHeight;
		m_cameraObj.transform.localPosition = new Vector3(m_cameraObj.transform.localPosition.x, m_curCameraHeight, m_curCameraDistance);
	}

	public void SetSwivelTarget(Transform target, TransmissionCharacterController controller)
	{
		m_swivelTarget = target;
		m_characterController = controller;
	}


	public void SetDesiredCameraPosition(float distanceZ, float height)
	{
		m_desiredCameraDistance = distanceZ;
		m_desiredCameraHeight = height;
	}

	public void SetDesiredCameraPositionByFraction(float distanceFrac, float heightFrac)
	{
		m_desiredCameraDistance = m_startingCameraDistance * distanceFrac;
		m_desiredCameraHeight = m_startingCameraHeight * heightFrac;
	}
	
	void LateUpdate()
	{
		//lock swivel position to target
		transform.position = m_swivelTarget.position;

		//lerp camera distance
		m_curCameraDistance = Mathf.Lerp(m_curCameraDistance, m_desiredCameraDistance, cameraPositionLerpFraction * Time.deltaTime * 30f);
		m_curCameraHeight = Mathf.Lerp(m_curCameraHeight, m_desiredCameraHeight, cameraPositionLerpFraction * Time.deltaTime * 30f);
		m_cameraObj.transform.localPosition = new Vector3(m_cameraObj.transform.localPosition.x, m_curCameraHeight, m_curCameraDistance);

		//force camera look
		if (cameraLookAtSwivel)
		{
			m_cameraObj.transform.rotation = Quaternion.LookRotation(transform.position - m_cameraObj.transform.position, m_characterController.KinematicCharacterMotor.CharacterUp);
		}

		//rotate towards velocity (check for 0 vel hitch bug and avoid drawing attention to it with camera)
		if (m_characterController.KinematicCharacterMotor.BaseVelocity.sqrMagnitude > 0f)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(m_characterController.KinematicCharacterMotor.BaseVelocity, m_characterController.KinematicCharacterMotor.CharacterUp), pitchLerpFraction * Time.deltaTime * 30f);
			Debug.DrawRay(transform.position, transform.forward * 10f, Color.cyan);
		}

		//debug to track sudden jerks in velocity
		float vel = Vector3.Distance(m_lastPos, transform.position) / Time.deltaTime;
		float badness = Mathf.Abs(vel - m_lastVel) / 2f;
		if (badness > .75f)
		{
			DebugUtil.DrawDebugCross(transform.position, 1f, Color.red, 100f);
		}
		Debug.DrawLine(m_lastPos, transform.position, Color.Lerp(Color.green, Color.red, badness), 100f);
		m_lastVel = vel;
		m_lastPos = transform.position;

	}
}
