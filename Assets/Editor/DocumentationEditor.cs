using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(Documentation), true)]
public class DocumentationEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		Documentation doc = target as Documentation;

		if (GUILayout.Button("Open Documentation"))
		{
			Application.OpenURL(doc.DocumenationUrl);
		}
	}
}
