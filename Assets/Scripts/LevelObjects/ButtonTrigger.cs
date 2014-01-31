using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ButtonTrigger : TriggerBase
{
	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onPressed = new List<SignalConnection>();

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onReleased = new List<SignalConnection>();

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onHold = new List<SignalConnection>();

	public string tag;
	public bool debug = false;
	public bool triggerOnce = false;

	private bool pressed;
	private bool objectInTrigger;

	void Start()
	{
		pressed = false;
		objectInTrigger = false;
	}

	void FixedUpdate()
	{
		if (objectInTrigger)
		{
			if (!pressed)
			{
				if (debug)
				{
					Debug.Log("On Pressed", this);
				}
				onPressed.ForEach(s => s.Fire());
			}
			else
			{
				if (debug)
				{
					Debug.Log("On Hold", this);
				}
				onHold.ForEach(s => s.Fire());
			}
			pressed = true;
		}
		else
		{
			if (pressed)
			{
				if (debug)
				{
					Debug.Log("On Released", this);
				}
				onReleased.ForEach(s => s.Fire());
			}
			pressed = false;
		}

		objectInTrigger = false;
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (tag.Length == 0 || other.attachedRigidbody.gameObject.tag == tag)
		{
			objectInTrigger = true;
		}
	}
}
