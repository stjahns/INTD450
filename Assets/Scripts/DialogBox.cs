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

	public bool showOnStart = false;
	public float showDelay = 1.0f;

	public float letterTime = 0.2f;
	public float showTime = 5.0f;

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

	public GUISkin skin;
	private string fullText;
	private string visibleText;

	private float maxHeight = 200;

	public enum DialogStyle
	{
		Normal,
		Instruction
	}

	public DialogStyle style = DialogStyle.Normal;

	void OnGUI()
	{
		bool shouldShow = state == DialogState.Showing || state == DialogState.Typing;
		shouldShow = shouldShow && Time.timeScale > 0; // Don't show when paused
		if (shouldShow)
		{

			string containerStyle = "DialogBoxContainer";
			string dialogStyle = "DialogBox";
			string textStyle = "DialogText";

			switch (style)
			{
				case DialogStyle.Instruction:
					textStyle = "DialogText_Inst";
					break;
			}


			// Height can't exceed maxHeight or half the screen vertical
			float height = Screen.height * 0.5f;
			height = Mathf.Min(height, maxHeight);

			GUI.skin = skin;
			GUI.depth = 1;

			GUILayout.BeginArea(new Rect(0, Screen.height - height, Screen.width, height));
			GUILayout.BeginVertical(containerStyle);
			GUILayout.BeginVertical(dialogStyle);
			GUILayout.Label(visibleText, textStyle);
			GUILayout.EndVertical();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}

	void Start()
	{
		if (skin == null)
		{
			skin = Resources.Load("ElectricGUI") as GUISkin;
		}

		prefix = "";
		if (speaker.Length > 0)
		{
			prefix = speaker + ": ";
		}

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
		//textObject.fontSize = (int)(Screen.width / fontToScreenWidthRatio);

		if (currentDialog)
		{
			currentDialog.Hide(true);
		}

		currentDialog = this;

		if (!suppressEvents)
		{
			onShow.ForEach(s => s.Fire());
		}

		fullText = prefix + dialogText;
		visibleText = prefix;

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

		state = DialogState.Hidden;
	}

	void Update()
	{
		// TODO need to adjust size according to viewport size, if a threshold is exceeded, use
		// a fixed size for the box

		//textObject.fontSize = (int)(Screen.width / fontToScreenWidthRatio);

		delayTimer += Time.deltaTime;

		if (state == DialogState.Hidden)
		{
			// DO NOTHING 
		}
		else if (state == DialogState.Unhiding)
		{
			// show and start typing after delay
			if (delayTimer > showDelay)
			{
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
					if (letterIndex < fullText.Length)
					{
						if (typeSound)
						{
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

			visibleText = fullText.Substring(0, letterIndex);
		}
		else if (state == DialogState.Showing)
		{
			visibleText = fullText;

			// Hide if enter hit or if nonzero showtime expires
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
					|| (showTime > 0 && delayTimer > showTime))
			{
				audio.PlayOneShot(skipSound, typeVolume);
				Hide();
			}
		}
	}
}
