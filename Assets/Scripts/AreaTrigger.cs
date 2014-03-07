using UnityEngine;
//using UnityEditor;
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
		if (debug)
		{
			Debug.Log("On Enter", this);
		}

		onEnter.ForEach(s => s.Fire());
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (debug)
		{
			Debug.Log("On Exit", this);
		}

		onExit.ForEach(s => s.Fire());
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (debug)
		{
			Debug.Log("On Stay", this);
		}

		onStay.ForEach(s => s.Fire());
	}
}
