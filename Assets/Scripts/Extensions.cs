using UnityEngine;

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
