using UnityEngine;
using System.Collections;

public class LevelEnd : MonoBehaviour
{
	public string nextLevel;

	//
	// Lets us pick an editor icon..
	//
	void OnDrawGizmos()
	{
	}

	//
	// When player enters LevelEnd trigger, proceed to next level
	//
	void OnTriggerEnter2D(Collider2D other)
	{
		int nextlevel = Application.loadedLevel + 1;
		Save_Load save = new Save_Load ();
		save.score = 200;
		save.level = nextlevel;
		save.player_name = "player";
		if (other.gameObject.tag == "Player")
		{
			save.create_new();
			Application.LoadLevel(nextlevel);
		}
	}
}
