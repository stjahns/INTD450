using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogBox : TriggerBase
{
	public string dialogText;

	public float wrapMargin;
	
	public bool showOnStart = false;
	public float showDelay = 1.0f;

	public float showTime = 5.0f;

	public bool enterToContinue = false;

	public GUIText textObject;
	public GUITexture backgroundObject;

	public static DialogBox currentDialog = null;

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onShow = new List<SignalConnection>();

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onHide = new List<SignalConnection>();

	private bool showing;
	private bool shownOnce = false;

	private float delayTimer;

	void Start ()
	{
		textObject.text = GetWrappedText(dialogText);
		textObject.enabled = false;
		backgroundObject.enabled = false;
		showing = false;

		// move to center...
		transform.position = Vector3.zero;

		delayTimer = 0.0f;
	}

	[InputSocket]
	public void Show()
	{
		// TODO might actually want to be able to delay showing when fired from event...
		Show(false);
	}

	public void Show(bool suppressEvents)
	{
		if (currentDialog)
		{
			currentDialog.Hide(true);
		}

		currentDialog = this;

		if (!suppressEvents)
		{
			onShow.ForEach(s => s.Fire());
		}

		textObject.enabled = true;
		backgroundObject.enabled = true;
		shownOnce = true;
		showing = true;
		delayTimer = 0.0f;
	}

	[InputSocket]
	public void Hide()
	{
		Hide(false);
	}

	public void Hide(bool suppressEvents)
	{
		currentDialog = null;

		if (!suppressEvents)
		{
			onHide.ForEach(s => s.Fire());
		}

		textObject.enabled = false;
		backgroundObject.enabled = false;
		showing = false;
	}
	
	void Update ()
	{
		// TODO need to adjust size according to viewport size, if a threshold is exceeded, use
		// a fixed size for the box

		// TODO -- when first showing, reveal one character at a time with an appropriate
		// sound effect...

		delayTimer += Time.deltaTime;

		if (showing)
		{
			textObject.text = GetWrappedText(dialogText);
			if (delayTimer > showTime)
			{
				Hide();
			}
		}
		else if (showOnStart && !shownOnce)
		{
			if (delayTimer > showDelay)
			{
				Show();
			}
		}
	}

	//
	// With the given string, return a new string with appropriate newlines to 
	// nicely wrap the text in the box
	//
	string GetWrappedText(string currentText)
	{
		float textWidth = Screen.width + backgroundObject.pixelInset.width - wrapMargin;

		string[] words = currentText.Split(' ');
		string finalText = "";

		GUIStyle style = new GUIStyle();
		style.font = textObject.guiText.font;
		style.fontSize = textObject.guiText.fontSize;
		style.fontStyle = textObject.guiText.fontStyle;

		int i = 0;
		for (int j = 0; j < words.Length; ++j)
		{
			// when from words from i to j exceed box width,
			// add i to j-1 with newline to final string

			string line = "";
			for (int word = i; word <= j; word++)
			{
				line += words[word] + " ";
			}

			float minWidth;
			float maxWidth;

			style.CalcMinMaxWidth(new GUIContent(line), out minWidth, out maxWidth);

			// TODO what if single word too wide for box?

			if (maxWidth > textWidth)
			{
				for (int word = i; word <= j; word++)
				{
					finalText += words[word] + " ";
				}

				finalText += "\n";

				i = j + 1;
			}
		}

		// Add remaining words on last line
		for (int word = i; word < words.Length; word++)
		{
			finalText += words[word] + " ";
		}

		return finalText;
	}
}
