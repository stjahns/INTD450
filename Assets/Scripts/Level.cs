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

			var player = spawnedPlayer.GetComponent<PlayerBehavior>();
			player.camera = playerCamera;
			player.OnDestroy += destroyedPlayer => {

				// Stop following player when it is destroyed
				playerCamera.target = playerCamera.transform;
				Debug.Log("CAMERA FREED");
			};
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
