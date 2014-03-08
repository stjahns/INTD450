using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FanBehaviour : MonoBehaviour
{
	public float fanForce = 10f;
	public bool fanOn;
	public DeathHazard fanHazard;
	public Animator fanAnimator;

	public Color editorColor;

	public float scalingDistance = 10000;

	private bool running;

	private Dictionary<int, List<Collider2D>> enteredBodies = 
		new Dictionary<int, List<Collider2D>>();

	public void OnDrawGizmos()
	{
		var boxCollider = collider2D as BoxCollider2D;
		if (boxCollider)
		{
			Gizmos.color = editorColor;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.zero + boxCollider.center.XY0(), 
					new Vector3(boxCollider.size.x, boxCollider.size.y, 1));

			Gizmos.DrawLine(boxCollider.center.XY0() + Vector3.up * boxCollider.size.y / 2,
					boxCollider.center.XY0() + Vector3.left * boxCollider.size.x / 2);

			Gizmos.DrawLine(boxCollider.center.XY0() - Vector3.up * boxCollider.size.y / 2,
					boxCollider.center.XY0() + Vector3.left * boxCollider.size.x / 2);
		}
	}

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

	void OnTriggerEnter2D(Collider2D other)
	{
		Rigidbody2D body = other.attachedRigidbody;
		if (body)
		{
			int bodyId = body.gameObject.GetInstanceID();
		
			if (enteredBodies.ContainsKey(bodyId))
			{
				// body already entered, but add new collider, if it doesn't already exist
				if (!enteredBodies[bodyId].Contains(other))
				{
					enteredBodies[bodyId].Add(other);
				}
			}
			else
			{
				// track new  body
				enteredBodies.Add(bodyId, new List<Collider2D>());
				enteredBodies[bodyId].Add(other);
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{

		Rigidbody2D body = other.attachedRigidbody;
		if (body)
		{
			int bodyId = body.gameObject.GetInstanceID();
			if (enteredBodies.ContainsKey(bodyId))
			{
				// remove collider
				enteredBodies[bodyId].Remove(other);
			}

			// Multiple colliders on same body could enter/exit area,
			// wait till next physics update to actually remove
		}
	}

	void FixedUpdate()
	{
		foreach (int bodyId in enteredBodies.Keys.ToList())
		{
			if (enteredBodies[bodyId].Count < 1)
			{
				// This body has fully left the area
				enteredBodies.Remove(bodyId);
			}
			else
			{
				// This body is sticking around
				// Add force proportional to distance and scaling distance...
				Rigidbody2D body = enteredBodies[bodyId][0].attachedRigidbody;

				float distance = Vector2.Distance(body.transform.position, transform.position);

				var boxCollider = collider2D as BoxCollider2D;

				scalingDistance = Mathf.Max(scalingDistance, 0.001f);
				float scale = 1 - (distance / scalingDistance);

				enteredBodies[bodyId][0].attachedRigidbody.AddForce(
						transform.rotation * Vector3.left * fanForce * scale);

				Debug.Log(scale);


			}
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
