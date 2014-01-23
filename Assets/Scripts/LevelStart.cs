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
	// Draw a green sphere at the spawn position
	//
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(0, 155, 0);
		Gizmos.DrawSphere(spawnPoint.position, 0.25f);
	}

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
