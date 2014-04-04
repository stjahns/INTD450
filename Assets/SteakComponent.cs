using UnityEngine;
using System.Collections;
using SimpleJSON;

public class SteakComponent : MonoBehaviour, SaveableComponent
{
	public void SaveState(JSONNode data)
	{
		data[gameObject.name]["x"].AsFloat = transform.position.x;
		data[gameObject.name]["y"].AsFloat = transform.position.y;
	}

	public void LoadState(JSONNode data)
	{
		if (data[gameObject.name] != null)
		{
			Vector2 position = new Vector2(data[gameObject.name]["x"].AsFloat,
										data[gameObject.name]["y"].AsFloat);
			StartCoroutine(DelayedLoad(position));
		}
	}

	IEnumerator DelayedLoad(Vector2 point)
	{
		// HACK -- since it might still be attached by a hinge joint to a change,
		// it might give a little resistance to being moved directly without some 
		// encouragement! Alternatively, we could destroy ourself and duplicate
		// ourself at the position...
		yield return 0;
		transform.position = point;
		yield return 0;
		transform.position = point;
		yield return 0;
		transform.position = point;
		yield return 0;
		transform.position = point;
	}
}
