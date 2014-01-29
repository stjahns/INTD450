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

}
