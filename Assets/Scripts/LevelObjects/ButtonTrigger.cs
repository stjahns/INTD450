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

	public bool debug = false;
	public bool triggerOnce = false;

	public Transform buttonContact;
	public Transform triggerContact;

	private bool pressed;

	public AudioClip pressClip;
	public AudioClip releaseClip;

	public SpriteRenderer spriteRenderer;
	public Sprite buttonUp;
	public Sprite buttonDown;


	void Start()
	{
		pressed = false;
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
	}

	void FixedUpdate()
	{
		if (!pressed)
		{
			// If buttonContact below trigger contact, pressed!
			if (buttonContact.position.y < triggerContact.position.y)
			{
				onPressed.ForEach(s => s.Fire());
				AudioSource.PlayClipAtPoint(pressClip, transform.position);
				pressed = true;
				spriteRenderer.sprite = buttonDown;
			}
		}
		else // held down
		{
			// If buttonContact above trigger contact, unpressed!
			if (buttonContact.position.y > triggerContact.position.y)
			{
				onReleased.ForEach(s => s.Fire());
				AudioSource.PlayClipAtPoint(releaseClip, transform.position);
				pressed = false;
				spriteRenderer.sprite = buttonUp;
			}
			else
			{
				onHold.ForEach(s => s.Fire());
			}
		}
	}
}
