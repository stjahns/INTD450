﻿using UnityEngine;
using System.Collections;

public class Level : MonoBehaviour 
{

	public LevelStart levelStart;
	public InGameMenu menu;

	//
	// Register events
	//
	void Awake ()
	{
		levelStart.PlayerSpawned += (spawner, spawnedPlayer) => {
			// When player is spawned, set follow camera to target player

			FollowCamera camera = Camera.main.GetComponent<FollowCamera>();
			if (camera)
			{
				var player = spawnedPlayer.GetComponent<PlayerBehavior>();

				camera.PushTarget(player.cameraTarget);

				player.OnDestroy += destroyedPlayer => {

					camera.PopTarget(player.cameraTarget);
					StartCoroutine(ShowMenu());

				};
			}
		};
	}

	[InputSocket]
	public void ResetLevel ()
	{
		// In the future, may want only reset key objects to not interrupt level 
		// animations, sounds and music
		// levelStart.ResetLevel();
		
		// For now, simply reload the level
		Application.LoadLevel(Application.loadedLevel);
	}

	IEnumerator ShowMenu()
	{
		yield return new WaitForSeconds(1.0f);
		menu.Show();
	}
	
}
