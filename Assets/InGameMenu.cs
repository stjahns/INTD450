using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InGameMenu : MonoBehaviour
{
	public List<GUIElement> elements;
	public List<InGameMenuButton> buttons;

	private bool active = false;

	void Start()
	{
		transform.position = Vector3.zero;
		elements.ForEach(e => e.enabled = false);
		buttons.ForEach(e => e.enabled = false);
		Time.timeScale = 1;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (!active)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
	}

	public void Show()
	{
		active = true;
		elements.ForEach(e => e.enabled = true);
		buttons.ForEach(e => e.enabled = true);
		Time.timeScale = 0;
	}


	public void Hide()
	{
		active = false;
		elements.ForEach(e => e.enabled = false);
		buttons.ForEach(e => e.enabled = false);
		Time.timeScale = 1;
	}
}
