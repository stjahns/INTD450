using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MenuButton : TriggerBase
{
	public Sprite click;
	public Sprite original;
	public AudioClip onclick;

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onClick = new List<SignalConnection>();

	void  OnMouseDown()
	{
		SpriteRenderer render = gameObject.GetComponent <SpriteRenderer>();
		AudioSource.PlayClipAtPoint (onclick, transform.position);

		double b = -20;
		Vector3 v = new Vector3((float)b,0,0);
		transform.Translate(v * 1 * Time.deltaTime); 
	}

	void  OnMouseUp()
	{
		SpriteRenderer render = gameObject.GetComponent <SpriteRenderer>();
		render.sprite = original;
		double b = 20;
		Vector3 v = new Vector3((float)b,0,0);
		transform.Translate(v * 1 * Time.deltaTime);

		onClick.ForEach(s => s.Fire());
	}
}
