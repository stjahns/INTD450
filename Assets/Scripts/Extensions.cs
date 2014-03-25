using UnityEngine;
using System.Collections;

public static class VectorExtensions
{
	// Extension method for converting a Vector3 to a Vector2
	public static Vector2 XY(this Vector3 v)
	{
		return new Vector2(v.x, v.y);
	}

	public static Vector3 XY0(this Vector2 v)
	{
		return new Vector3(v.x, v.y, 0);
	}
}

public static class TransformExtensions
{
	public static Transform rootParent(this Transform t)
	{
		if (t.parent == null)
		{
			return t;
		}
		else
		{
			return t.parent.rootParent();
		}
	}
}

public static class CoroutineExtensions
{
	public static IEnumerator WaitForRealSeconds(float time)
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}
}

public static class Log
{
	public static void Format(string msg, params object[] args)
	{
		Debug.Log(string.Format(msg, args));
	}

	public static void Format(Object context, string msg, params object[] args)
	{
		Debug.Log(string.Format(msg, args), context);
	}
}
