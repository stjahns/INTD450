using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InGameMenuButton : TriggerBase
{
	public AudioClip onclick;
	public AudioClip mouseEnter;
	public AudioClip mouseExit;

	public GUIText guiText;

	public float fontToScreenWidthRatio = 20;

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onClick = new List<SignalConnection>();

	private Vector3 originalScale;

	private bool mouseEntered = false;
	private bool mouseDown = false;

	void OnGUI()
	{
		GUITexture button = GetComponent<GUITexture>();

		if (button.HitTest(Input.mousePosition))
		{
			if (!mouseEntered)
			{
				OnMouseEnter();
				mouseEntered = true;
			}

			if (Input.GetMouseButtonDown(0))
			{
				OnMouseDown();
				mouseDown = true;
			}
		}
		else
		{
			if (mouseEntered)
			{
				OnMouseExit();
				mouseEntered = false;
			}
		}

		if (mouseDown && Input.GetMouseButtonUp(0))
		{
			OnMouseUp();
			mouseDown = false;
		}
	}

	virtual public void Awake()
	{
		originalScale = transform.localScale;

		foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
		{
			renderer.sortingLayerName = "UI";
			renderer.sortingOrder = 1;
		}

		guiText.fontSize = (int) (Screen.width / fontToScreenWidthRatio);
	}

	virtual public void Update()
	{
		guiText.fontSize = (int) (Screen.width / fontToScreenWidthRatio);
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
