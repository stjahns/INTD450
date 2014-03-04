using UnityEngine;
using System.Collections;

public class SawController : MonoBehaviour
{
	public SpriteRenderer sawblade;
	public float bladeTopSpeed;
	public float bladeAcceleration;

	public Collider2D bladeTrigger;

	public AudioSource sawRunning; 
	public AudioSource sawHitting; 

	private bool running;
	private float bladeSpeed;

	void Start ()
	{
		running = false;
		bladeSpeed = 0;
	}

	//
	// Accelerate / decelerate blade if running / stopped
	// Rotate blade at current bladeSpeed
	// 
	void Update ()
	{
		if (running)
		{
			if (bladeSpeed < bladeTopSpeed)
			{
				bladeSpeed += bladeAcceleration * Time.deltaTime;
			}

			if (bladeSpeed > bladeTopSpeed)
			{
				bladeSpeed = bladeTopSpeed;
			}

		}
		else
		{
			if (bladeSpeed > 0.0f)
			{
				bladeSpeed -= bladeAcceleration * Time.deltaTime;
			}

			if (bladeSpeed < 0)
			{
				bladeSpeed = 0;
			}
		}

		// bladeSpeed in degrees per second
		if (bladeSpeed > 0)
		{
			float rotation = bladeSpeed * Time.deltaTime;
			sawblade.transform.Rotate(0, 0, rotation);
		}
	}

	[InputSocket]
	public void SawOn()
	{
		// start rotating sawblade
		running = true;
		sawRunning.Play();
		bladeTrigger.enabled = true;
	}

	[InputSocket]
	public void SawOff()
	{
		// stop rotating sawblade
		running = false;
		sawRunning.Stop();
		bladeTrigger.enabled = false;
	}
}
