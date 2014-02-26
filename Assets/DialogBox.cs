using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogBox : TriggerBase
{
	public enum DialogState
	{
		Hidden,
		Typing,
		Showing
	};

	public DialogState state = DialogState.Hidden;

	public string speaker;
	public string dialogText;

	public float wrapMargin;
	
	public bool showOnStart = false;
	public float showDelay = 1.0f;

	public float fontToScreenWidthRatio = 20.0f;

	public float letterTime = 0.2f;
	public float showTime = 5.0f;

	public bool enterToContinue = false;

	public GUIText textObject;
	public GUITexture backgroundObject;

	public static DialogBox currentDialog = null;

	public AudioClip typeSound;

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onShow = new List<SignalConnection>();

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onHide = new List<SignalConnection>();

	private bool shownOnce = false;

	private int letterIndex;
	private float letterTimer;

	private float delayTimer;

	private string currentText;

	void Start ()
	{
		textObject.text = GetWrappedText(dialogText);
		textObject.enabled = false;
		backgroundObject.enabled = false;

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

		currentText = speaker + ": ";
		textObject.enabled = true;
		backgroundObject.enabled = true;
		shownOnce = true;
		delayTimer = 0.0f;
		letterTimer = 0.0f;
		letterIndex = 0;

		state = DialogState.Typing;
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
		state = DialogState.Hidden;
	}
	
	void Update ()
	{
		// TODO need to adjust size according to viewport size, if a threshold is exceeded, use
		// a fixed size for the box

		textObject.fontSize = (int) (Screen.width / fontToScreenWidthRatio);

		delayTimer += Time.deltaTime;

		if (state == DialogState.Hidden)
		{
			// If supposed to show on start, and haven't yet, show after delay
			if (showOnStart && !shownOnce)
			{
				if (delayTimer > showDelay)
				{
					Show();
				}
			}
		}
		else if (state == DialogState.Typing)
		{
			// Reveal letters one by one
			letterTimer += Time.deltaTime;
			if (letterTimer > letterTime)
			{
				if (letterIndex < dialogText.Length)
				{
					currentText += dialogText[letterIndex];
					if (typeSound)
					{
						AudioSource.PlayClipAtPoint(typeSound, transform.position);
					}
				}
				else
				{
					// Fully revealed, go to Showing state
					state = DialogState.Showing;
					delayTimer = 0.0f;
				}

				letterIndex++;
				letterTimer = 0;
			}

			textObject.text = GetWrappedText(currentText);

		}
		else if (state == DialogState.Showing)
		{
			// Hide after show time expires
			if (delayTimer > showTime)
			{
				Hide();
			}
		}
	}

	//
	// With the given string, return a new string with appropriate newlines to 
	// nicely wrap the text in the box
	//
	string GetWrappedText(string text)
	{
		float textWidth = Screen.width + backgroundObject.pixelInset.width - wrapMargin;

		string[] words = text.Split(' ');
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
