using UnityEngine;
using System.Collections;

public class FanBehaviour : MonoBehaviour
{
	public float fanForce = 10f;
	public bool fanOn;
	public DeathHazard fanHazard;
	public Animator fanAnimator;

	void Start()
	{
		fanAnimator.SetBool("Running", fanOn);
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (fanOn)
		{
			other.attachedRigidbody.AddForce(transform.rotation * Vector3.left * fanForce);
		}
	}

	[InputSocket]
	public void FanOn()
	{
		fanOn = true;
		
		if (fanHazard)
		{
			fanHazard.enabled = true;
		}

		fanAnimator.SetBool("Running", true);

	}

	[InputSocket]
	public void FanOff()
	{
		fanOn = false;

		if (fanHazard)
		{
			fanHazard.enabled = false;
		}

		fanAnimator.SetBool("Running", false);
	}
}
