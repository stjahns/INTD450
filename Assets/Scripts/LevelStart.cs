using UnityEngine;
using System.Collections;

public class LevelStart : MonoBehaviour
{

	public GameObject playerPrefab;
	public PlayerBehavior player;
	public Transform spawnPoint;

	public delegate void PlayerSpawnedHandler(GameObject spawner, GameObject spawnedObject);
	public event PlayerSpawnedHandler PlayerSpawned;

	//
	// On level start, spawn player
	//
	void Start ()
	{
		SpawnPlayer();
	}

	//
	// Instantiates the playerPrefab at the spawnPoint
	//
	public void SpawnPlayer()
	{
		var spawnedPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
		player = (spawnedPlayer as GameObject).GetComponent<PlayerBehavior>();
		
		if (PlayerSpawned != null)
		{
			PlayerSpawned(this.gameObject, player.gameObject);
		}
	}

	//
	// Destroy previously spawned player and respawn
	//
	public void Reset()
	{
		if (player != null)
		{
			Destroy(player.gameObject);
			player = null;
		}

		SpawnPlayer();
	}
}
