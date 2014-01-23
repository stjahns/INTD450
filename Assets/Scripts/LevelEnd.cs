using UnityEngine;
using System.Collections;

public class LevelEnd : MonoBehaviour
{
	public string nextLevel;

	//
	// Draw a red sphere to indicate this is level end
	//
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(155, 0, 0);
		Gizmos.DrawSphere(transform.position, 0.25f);
	}

	//
	// When player enters LevelEnd trigger, proceed to next level
	//
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			Application.LoadLevel(nextLevel);
		}
	}
}
