using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AreaTrigger : MonoBehaviour
{
	public List<SignalConnection> onEnter = new List<SignalConnection>();
	public List<SignalConnection> onExit = new List<SignalConnection>();
	public List<SignalConnection> onStay = new List<SignalConnection>();

	public string tag;
	public bool debug = false;
	public bool triggerOnce = false;

	public BoxCollider2D collider;
	public Color color;

	void OnDrawGizmos()
	{
		Gizmos.color = color;
		Gizmos.DrawCube(transform.position, new Vector3(collider.size.x, collider.size.y, 1));
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (tag.Length == 0 || other.attachedRigidbody.gameObject.tag == tag)
		{
			if (debug)
			{
				Debug.Log("On Enter", this);
			}

			onEnter.ForEach(s => s.Fire());
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (tag.Length == 0 || other.attachedRigidbody.gameObject.tag == tag)
		{
			if (debug)
			{
				Debug.Log("On Exit", this);
			}

			onExit.ForEach(s => s.Fire());
		}
	}
}
