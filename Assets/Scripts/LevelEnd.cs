using UnityEngine;
using System.Collections;

public class LevelEnd : MonoBehaviour
{
	public string nextLevel;

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
