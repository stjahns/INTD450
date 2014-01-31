using UnityEngine;
using System.Collections;

public class PlayerGate : MonoBehaviour
{

	public bool AllowAttachedLimbs;

	void OnDrawGizmos()
	{
		// Empty to allow us to set an editor icon...
	}

	[InputSocket]
	public void Open()
	{
		if (AllowAttachedLimbs || !PlayerBehavior.Player.HasLimbs)
		{
			collider2D.enabled = false;
		}
	}

	[InputSocket]
	public void Close()
	{
		collider2D.enabled = true;
	}

}

