using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrapZone : MonoBehaviour
{
	public Transform captureCenter;
	public List<Rigidbody2D> capturedBodies = new List<Rigidbody2D>();

	public float moveSpeedModifier = 1;
	public float maxMoveSpeed = 1;
	public float moveAccelGain = 1;
	public float maxMoveForce = 1;

	public float explodeDelay = 1;
	public float randomDelayRange = 1;

	void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.attachedRigidbody && !capturedBodies.Contains(collider.attachedRigidbody))
		{
			capturedBodies.Add(collider.attachedRigidbody);
		}
	}

	void FixedUpdate()
	{
		for (int i = 0; i < capturedBodies.Count; ++i)
		{
			var body = capturedBodies[i];
			if (body)
			{
				Vector3 moveTarget = captureCenter.position;
				Vector2 toMoveTarget = moveTarget - body.transform.position;

				// Calculate target velocity, proportional to distance, but capped at max speed
				Vector2 targetVelocity = Vector2.ClampMagnitude(moveSpeedModifier * toMoveTarget, maxMoveSpeed);
				Vector2 velocityError = targetVelocity - body.velocity;

				// Force proportional to gain and velocity error, capped at max force
				Vector2 force = Vector2.ClampMagnitude(moveAccelGain * velocityError, maxMoveForce);

				// Apply the force
				body.AddForce(force);

				if (body.name != "Boss")
				{
					// counteract gravity..
					body.AddForce(-Physics2D.gravity * body.mass);
				}
			}
		}
	}

	[InputSocket]
	public void ExpodeContents()
	{
		for (int i = 0; i < capturedBodies.Count; ++i)
		{
			var body = capturedBodies[i];
			if (body)
			{
				StartCoroutine(ExplodeRoutine(body));
			}
		}
	}

	IEnumerator ExplodeRoutine(Rigidbody2D body)
	{
		yield return new WaitForSeconds(Random.Range(explodeDelay - randomDelayRange,
					explodeDelay + randomDelayRange));
		if (body)
		{
			body.SendMessage("Explode", SendMessageOptions.DontRequireReceiver);
			body.SendMessage("DestroyRobotComponent", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnDrawGizmos()
	{
	}
}
