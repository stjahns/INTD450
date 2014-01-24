using UnityEngine;
using System.Collections;

public class FanBehaviour : MonoBehaviour
{
	public float fanForce = 10f;

	void OnTriggerStay2D(Collider2D other)
	{
		other.attachedRigidbody.AddForce(transform.rotation * Vector3.left * fanForce);
	}
}
