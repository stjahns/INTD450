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
		Save_Load save = new Save_Load ();
		save.score = 200;
		save.level = 1;
		save.player_name = "player";
		if (other.gameObject.tag == "Player")
		{
			save.create_new();
			Application.LoadLevel(nextLevel);
		}
	}
}
