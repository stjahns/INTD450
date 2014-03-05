using UnityEngine;
using System.Collections;

public class FanBehaviour : MonoBehaviour
{
	public float fanForce = 10f;
	public bool fanOn;
	public DeathHazard fanHazard;
	public Animator fanAnimator;

	private bool running;

	void Start()
	{
		if (fanOn)
		{
			FanOn();
		}
		else
		{
			FanOff();
		}
	}

	void Update()
	{
		if (!running && fanOn)
		{
			FanOn();
		}

		if (running && !fanOn)
		{
			FanOff();
		}
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
		audio.Play();

		running = true;

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
		audio.Stop();

		running = false;
	}
}
