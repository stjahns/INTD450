using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogBox : TriggerBase
{
	public enum DialogState
	{
		Hidden,
		Unhiding,
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

	public GUIText textObject;
	public GUITexture backgroundObject;

	public static DialogBox currentDialog = null;

	public AudioClip typeSound;
	public AudioClip skipSound;

	[Range(0, 1)]
	public float typeVolume;

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onShow = new List<SignalConnection>();

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onHide = new List<SignalConnection>();

	private int letterIndex;
	private float letterTimer;

	private float delayTimer;

	private string prefix;
	private string wrappedText;

	void Start ()
	{

		prefix = "";
		if (speaker.Length > 0)
		{
			prefix = speaker + ": ";
		}

		textObject.enabled = false;
		backgroundObject.enabled = false;

		// move gui elements to center... kinda hacky...
		Vector3 position = transform.position;
		transform.position = Vector3.zero;
		textObject.transform.parent = null;
		backgroundObject.transform.parent = null;
		transform.position = position;

		delayTimer = 0.0f;

		if (showOnStart)
		{
			Show();
		}
	}

	[InputSocket]
	public void Show()
	{
		// TODO might actually want to be able to delay showing when fired from event...
		Show(false);
	}

	public void Show(bool suppressEvents)
	{
		textObject.fontSize = (int) (Screen.width / fontToScreenWidthRatio);

		if (currentDialog)
		{
			currentDialog.Hide(true);
		}

		currentDialog = this;

		if (!suppressEvents)
		{
			onShow.ForEach(s => s.Fire());
		}

		wrappedText = GetWrappedText(prefix + dialogText);
		textObject.text = wrappedText.Substring(0, prefix.Length);
		
		delayTimer = 0.0f;
		letterTimer = 0.0f;
		letterIndex = prefix.Length;

		state = DialogState.Unhiding;
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
			// DO NOTHING 
		}
		else if (state == DialogState.Unhiding)
		{
			// enable visual elements and start typing after delay
			if (delayTimer > showDelay)
			{
				textObject.enabled = true;
				backgroundObject.enabled = true;
				state = DialogState.Typing;
			}
		}
		else if (state == DialogState.Typing)
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				// Skip typing, fully reveal
				if (skipSound)
				{
					//audio.PlayOneShot(skipSound, typeVolume);
					AudioSource3D.PlayClipOmnipresent(typeSound, typeVolume);
				}

				state = DialogState.Showing;
				delayTimer = 0.0f;
			}
			else
			{
				// Reveal letters one by one
				letterTimer += Time.deltaTime;
				if (letterTimer > letterTime)
				{
					if (letterIndex < wrappedText.Length)
					{
						if (typeSound)
						{
							//audio.PlayOneShot(typeSound, typeVolume);
							AudioSource3D.PlayClipOmnipresent(typeSound, typeVolume);
						}
					}
					else
					{
						// Fully revealed, go to Showing state
						state = DialogState.Showing;
						letterIndex--;
						delayTimer = 0.0f;
					}

					letterIndex++;
					letterTimer = 0;
				}
			}

			textObject.text = wrappedText.Substring(0, letterIndex);
		}
		else if (state == DialogState.Showing)
		{
			textObject.text = wrappedText;

			// Hide if enter hit or if nonzero showtime expires
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
					|| (showTime > 0 && delayTimer > showTime))
			{
				audio.PlayOneShot(skipSound, typeVolume);
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

		float minWidth;
		float maxWidth;

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

			style.CalcMinMaxWidth(new GUIContent(line), out minWidth, out maxWidth);

			// TODO what if single word too wide for box?

			if (maxWidth > textWidth)
			{
				for (int word = i; word < j; word++)
				{
					finalText += words[word] + " ";
				}

				finalText += "\n";

				i = j;
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
