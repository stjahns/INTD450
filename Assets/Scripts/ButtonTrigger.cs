using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ButtonTrigger : MonoBehaviour
{
	public List<SignalConnection> onPressed = new List<SignalConnection>();
	public List<SignalConnection> onReleased = new List<SignalConnection>();
	public List<SignalConnection> onHold = new List<SignalConnection>();

	public string tag;
	public bool debug = false;
	public bool triggerOnce = false;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (tag.Length == 0 || other.attachedRigidbody.gameObject.tag == tag)
		{
			if (debug)
			{
				Debug.Log("On Pressed", this);
			}

			onPressed.ForEach(s => s.Fire());
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (tag.Length == 0 || other.attachedRigidbody.gameObject.tag == tag)
		{
			if (debug)
			{
				Debug.Log("On Released", this);
			}

			onReleased.ForEach(s => s.Fire());
		}
	}
}
