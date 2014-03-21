using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MenuButton : TriggerBase
{
	public AudioClip onclick;
	public AudioClip mouseEnter;
	public AudioClip mouseExit;

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onClick = new List<SignalConnection>();

	private Vector3 originalScale;

	virtual public void Start()
	{
		originalScale = transform.localScale;
	}

	virtual public void OnMouseEnter()
	{
		AudioSource.PlayClipAtPoint (mouseEnter, transform.position);
		transform.localScale = originalScale * 1.1f;
	}

	virtual public void OnMouseExit()
	{
		AudioSource.PlayClipAtPoint (mouseExit, transform.position);
		transform.localScale = originalScale;
	}

	virtual public void OnMouseDown()
	{
		AudioSource.PlayClipAtPoint (onclick, transform.position);
		transform.localScale = originalScale * 0.9f;

	}

	virtual public void OnMouseUp()
	{
		transform.localScale = originalScale;
		onClick.ForEach(s => s.Fire());
	}
}
