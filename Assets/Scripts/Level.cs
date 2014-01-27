using UnityEngine;
using System.Collections;

public class Level : MonoBehaviour 
{

	public FollowCamera playerCamera;
	public LevelStart levelStart;

	//
	// Register events
	//
	void Awake ()
	{
		levelStart.PlayerSpawned += (spawner, spawnedPlayer) => {
			// When player is spawned, set follow camera to target player
			playerCamera.target = spawnedPlayer.transform;
		};
	}

	//
	// Reset level when reset button hit
	//
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			ResetLevel();
		}
	}

	public void ResetLevel ()
	{
		// In the future, may want only reset key objects to not interrupt level 
		// animations, sounds and music
		// levelStart.ResetLevel();
		
		// For now, simply reload the level
		Application.LoadLevel(Application.loadedLevel);
	}

	
}
