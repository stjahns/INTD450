using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuzzsawComponent : LimbComponent
{
	public SpriteRenderer sawblade;
	public float bladeTopSpeed;
	public float bladeAcceleration;
	public float bladeForce;
	public float groundAngleTolerance = 45;
	public Transform forceDirection;

	public Collider2D bladeTrigger;

	public AudioSource sawRunning; 
	public AudioSource sawHitting; 

	private bool running;
	private float bladeSpeed;

	override public void Start ()
	{
		base.Start();
		running = false;
		bladeSpeed = 0;
		bladeTrigger.enabled = false;
	}

	//
	// Accelerate / decelerate blade if running / stopped
	// Rotate blade at current bladeSpeed
	// 
	override public void Update ()
	{
		base.Update();

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

			if (Skeleton && Skeleton.direction == PlayerSkeleton.Direction.Right)
			{
				rotation *= -1;
			}

			sawblade.transform.Rotate(0, 0, rotation);
		}
	}

	//
	// If running, and in contact with ground, apply force on rigid body
	// 
	override public void FixedUpdate ()
	{
		base.FixedUpdate();

		if (running)
		{
			int layerMask = 1 << LayerMask.NameToLayer("Ground");
			layerMask |= 1 << LayerMask.NameToLayer("HookWall");

			RaycastHit2D hit = Physics2D.Linecast(transform.position, groundCheck.position, layerMask);
			if (hit)
			{
				Debug.Log(LayerMask.LayerToName(hit.collider.gameObject.layer));

				// Unless angle within ground tolerance, only bite in if it's a hookwall
				if (Vector2.Angle(Vector2.up, hit.normal) < groundAngleTolerance
						|| hit.collider.gameObject.layer == LayerMask.NameToLayer("HookWall"))
				{
					Vector3 direction = forceDirection.position - sawblade.transform.position;
					direction.Normalize();
					
					if (getRootComponent().rigidbody2D)
					{
						getRootComponent().rigidbody2D.AddForce(bladeForce * direction);
					}

					if (!sawHitting.isPlaying)
					{
						sawHitting.Play();
					}
				}
			}
			else
			{
				if (sawHitting.isPlaying)
				{
					sawHitting.Stop();
				}
			}
		}
		else
		{
			if (sawHitting.isPlaying)
			{
				sawHitting.Stop();
			}
		}
	}

	//
	// Toggle blade on / off
	// 
	override public void FireAbility()
	{
		if (!running)
		{
			// start rotating sawblade
			running = true;

			SawbladeController blade = GetComponentInChildren<SawbladeController>();
			if (blade)
			{
				blade.Running = true;
			}

			sawRunning.Play();
			bladeTrigger.enabled = true;
		}
		else
		{
			// stop rotating sawblade
			running = false;

			SawbladeController blade = GetComponentInChildren<SawbladeController>();
			if (blade)
			{
				blade.Running = false;
			}

			sawRunning.Stop();
			bladeTrigger.enabled = false;
		}
	}

	override public void OnRemove()
	{
		base.OnRemove();

		// stop rotating sawblade
		running = false;

		SawbladeController blade = GetComponentInChildren<SawbladeController>();
		if (blade)
		{
			blade.Running = false;
		}

		sawRunning.Stop();
		bladeTrigger.enabled = false;
	}
}
