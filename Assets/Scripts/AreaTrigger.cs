using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

public class AreaTrigger : TriggerBase
{
	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onEnter = new List<SignalConnection>();

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onExit = new List<SignalConnection>();

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onStay = new List<SignalConnection>();

	public bool debug = false;

	public Color color;

	public List<string> tags = new List<string>();

	private Dictionary<int, List<Collider2D>> enteredBodies = 
		new Dictionary<int, List<Collider2D>>();

	override public void OnDrawGizmos()
	{
		base.OnDrawGizmos();

		var boxCollider = collider2D as BoxCollider2D;
		if (boxCollider)
		{
			Gizmos.color = color;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.zero + boxCollider.center.XY0(), 
					new Vector3(boxCollider.size.x, boxCollider.size.y, 1));
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		// TODO check tags

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

				if (debug)
				{
					Debug.Log("On Enter", this);
				}

				onEnter.ForEach(s => s.Fire());
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
			// wait till next physics update to actually fire on exit
		}
	}

	void FixedUpdate()
	{
		foreach (int bodyId in enteredBodies.Keys.ToList())
		{
			if (enteredBodies[bodyId].Count < 1)
			{
				// This body has fully left the area
				if (debug)
				{
					Debug.Log("On Exit", this);
				}

				onExit.ForEach(s => s.Fire());

				enteredBodies.Remove(bodyId);
			}
			else
			{
				// This body is sticking around
				if (debug)
				{
					Debug.Log("On Stay", this);
				}

				onStay.ForEach(s => s.Fire());
			}
		}

	}
}
