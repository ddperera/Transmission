using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UngroundTrigger : MonoBehaviour {

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			other.gameObject.GetComponent<TransmissionCharacterController>().ForceUnground();
		}
	}

}
