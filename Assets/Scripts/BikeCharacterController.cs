using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController
{
	public class BikeCharacterController : BaseCharacterController
	{
		[Header("Stable Movement")]
		public float MaxStableMoveSpeed = 10f; // Max speed when stable on ground
		public float StableMovementSharpness = 15; // Sharpness of the acceleration when stable on ground
		public float OrientationSharpness = 10; // Sharpness of rotations when stable on ground
		public float SlerpSpeed =7f;

		[Header("Air Movement")]
		public float MaxAirMoveSpeed = 10f; // Max speed for air movement
		public float AirAccelerationSpeed = 5f; // Acceleration when in air
		public float Drag = 0.1f; // Air drag

		[Header("Misc")]
		public bool OrientTowardsGravity = true; // Should the character align its up direction with the gravity (used for the planet example)
		public Vector3 Gravity = new Vector3(0, -9.81f, 0); // Gravity vector

		[HideInInspector]
		public Collider[] IgnoredColliders = new Collider[] { };

		private Collider[] _probedColliders = new Collider[8];
		private Vector3 _moveInputVector = Vector3.zero;
		private Vector3 _lookInputVector = Vector3.zero;
		private Vector3 _smoothedLookInputDirection = Vector3.zero;
		private Vector3 _internalVelocityAdd = Vector3.zero;

		/// <summary>
		/// This is called by the ExamplePlayer or the ExampleAIController to set the character's movement and look input vectors
		/// </summary>
		public void SetInputs(Vector3 moveInput, Vector3 lookInput)
		{
			_moveInputVector = Vector3.ProjectOnPlane(moveInput, KinematicCharacterMotor.CharacterUp).normalized * moveInput.magnitude;
			_lookInputVector = Vector3.ProjectOnPlane(lookInput, KinematicCharacterMotor.CharacterUp).normalized;
		}

		public override void BeforeCharacterUpdate(float deltaTime)
		{
		}

		public override bool MustUpdateGrounding()
		{
			// In this case, we always want to probe for ground. However, if we wanted to add a swimming 
			// movement mode for example, we wouldn't want to probe and snap to ground while we are swimming. In 
			// that case we would return false here
			return true;
		}

		public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
		{
			Debug.DrawRay(transform.position, _lookInputVector * 5f, Color.red);
			if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
			{
				_smoothedLookInputDirection = Vector3.Slerp(_smoothedLookInputDirection, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
				currentRotation = Quaternion.LookRotation(_smoothedLookInputDirection, KinematicCharacterMotor.CharacterUp);
			}
			if (OrientTowardsGravity)
			{
				currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -Gravity) * currentRotation;
			}

			Quaternion targetRotation = Quaternion.FromToRotation ((currentRotation * Vector3.up), KinematicCharacterMotor.GroundNormal) * currentRotation;
			currentRotation = Quaternion.Slerp (currentRotation, targetRotation, 1 - Mathf.Exp(-SlerpSpeed * deltaTime));
		}

		public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
		{
			Vector3 targetMovementVelocity = Vector3.zero;
			if (KinematicCharacterMotor.IsStableOnGround)
			{
				// Reorient velocity on slope
				currentVelocity = KinematicCharacterMotor.GetDirectionTangentToSurface(currentVelocity, KinematicCharacterMotor.GroundNormal) * currentVelocity.magnitude;

				// Calculate target velocity
				Vector3 inputRight = Vector3.Cross(_moveInputVector, KinematicCharacterMotor.CharacterUp);
				Vector3 reorientedInput = Vector3.Cross(KinematicCharacterMotor.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
				targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

				// Independant movement Velocity
				currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
			}
			else
			{
				// Add move input
				if (_moveInputVector.sqrMagnitude > 0f)
				{
					targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;
					Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
					currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
				}

				// Gravity
				currentVelocity += Gravity * deltaTime;

				// Drag
				currentVelocity *= (1f / (1f + (Drag * deltaTime)));
			}
				
			// Take into account additive velocity
			if (_internalVelocityAdd.sqrMagnitude > 0f)
			{
				currentVelocity += _internalVelocityAdd;
				_internalVelocityAdd = Vector3.zero;
			}
		}

		public override void AfterCharacterUpdate(float deltaTime)
		{
			// Grounding considerations
			if (KinematicCharacterMotor.IsStableOnGround && !KinematicCharacterMotor.WasStableOnGround)
			{
				OnLanded();
			}
			else if (!KinematicCharacterMotor.IsStableOnGround && KinematicCharacterMotor.WasStableOnGround)
			{
				OnLeaveStableGround();
			}
		}

		public override bool IsColliderValidForCollisions(Collider coll)
		{
			// Example of ignoring collisions with specific colliders
			for(int i = 0; i < IgnoredColliders.Length; i++)
			{
				if(coll == IgnoredColliders[i])
				{
					return false;
				}
			}

			return true;
		}

		public override bool CanBeStableOnCollider(Collider coll)
		{
			return true;
		}

		public override void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, bool isStableOnHit)
		{
		}

		public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, bool isStableOnHit)
		{
		}

		public void AddVelocity(Vector3 velocity)
		{
			_internalVelocityAdd += velocity;
		}

		protected void OnLanded()
		{
		}

		protected void OnLeaveStableGround()
		{
		}
		
	}
}

