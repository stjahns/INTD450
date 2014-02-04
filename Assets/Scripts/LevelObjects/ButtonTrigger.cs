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

	public AudioClip pressClip;
	public AudioClip releaseClip;

	public SpriteRenderer buttonUp;
	public SpriteRenderer buttonDown;

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
				AudioSource.PlayClipAtPoint(pressClip, transform.position);

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

			buttonUp.enabled = false;
			buttonDown.enabled = true;
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
				AudioSource.PlayClipAtPoint(releaseClip, transform.position);
			}
			pressed = false;

			buttonUp.enabled = true;
			buttonDown.enabled = false;
		}

		objectInTrigger = false;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (tag.Length == 0 || other.attachedRigidbody.gameObject.tag == tag)
		{
			objectInTrigger = true;
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (tag.Length == 0 || other.attachedRigidbody.gameObject.tag == tag)
		{
			objectInTrigger = true;
		}
	}
}
