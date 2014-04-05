using UnityEngine;
using UnityEditor;
using System.Collections;

public class CustomCommands : MonoBehaviour {

	[MenuItem("Level/Group Objects %g")]
	static void GroupObjects()
	{
		if (EditorUtility.DisplayDialog("Group Objects",
				"Parent selection under a common object?",
				"Ok", "Cancel"))
		{
			GameObject parentObject = new GameObject("Group");

			foreach (GameObject obj in Selection.gameObjects)
			{
				obj.transform.parent = parentObject.transform;
			}

			Selection.activeGameObject = parentObject;
		}
	}

	[MenuItem("Level/Snap Objects %UP")]
	static void SnapObjects()
	{
		foreach (GameObject obj in Selection.gameObjects)
		{
			Vector3 position = obj.transform.position;

			if (position.x % 1.28f < 0.64f)
			{
				position.x -= position.x % 1.28f;
			}
			else
			{
				position.x += 1.28f - position.x % 1.28f;
			}

			if (position.y % 1.28f < 0.64f)
			{
				position.y -= position.y % 1.28f;
			}
			else
			{
				position.y += 1.28f - position.y % 1.28f;
			}

			position.z = 0;

			obj.transform.position = position;
		}
	}

	[MenuItem("Level/Toggle Trigger Connections %DOWN")]
	static void ToggleShowConnections()
	{
		TriggerBase.DrawLines = !TriggerBase.DrawLines;
	}

}
