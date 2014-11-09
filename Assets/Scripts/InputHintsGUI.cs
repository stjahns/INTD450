using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputHintsGUI : MonoBehaviour 
{
	public Vector2 screenPos = new Vector2(50, 50);
	public Vector2 size = new Vector2(400, 400);
	public GUISkin skin;

	public class InputHint
	{
		public string Key;
		public string Description;

		public InputHint(string key, string description)
		{
			this.Key = key;
			this.Description = description;
		}
	}

	public List<InputHint> inputHints = new List<InputHint>();

	virtual public void OnDrawGizmos()
	{
	}

	// Public Interface

	public void ClearHints()
	{
		inputHints.Clear();
	}

	public void AddHint(InputHint hint)
	{
		inputHints.Add(hint);
	}

	public void AddHint(string key, string description)
	{
		inputHints.Add(new InputHint(key, description));
	}

	void Start()
	{
		skin = Resources.Load("ElectricGUI") as GUISkin;
	}

	void OnGUI()
	{
		if (enabled)
		{
			GUI.skin = skin;
			GUI.depth = -1;

			GUILayout.BeginArea(new Rect(screenPos.x, screenPos.y, size.x, size.y));

			GUILayout.BeginVertical("HintBox");

			foreach (var hint in inputHints)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(hint.Key, "HintKey");
				GUILayout.Label(hint.Description, "HintDescription");
				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();

			GUILayout.EndArea();
		}
	}
}
