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
	// Lets us pick an editor icon..
	//
	void OnDrawGizmos()
	{
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
		Save_Load load = new Save_Load ();
		load.player_name="player";
		var data = load.file_load ();
		string flag="";
		int level = 0;
		flag = data ["array"][1]["checkpoint"];
		level = System.Convert.ToInt32(data ["array"][1]["Level"]);
		if (flag != "Null" && level == Application.loadedLevel) 
		{	///load.create_new();
			flag = flag.Replace('(',' ');
			flag = flag.Replace(')',' ');
			string[] vecotr = flag.Split(',');
			float x= (float)System.Convert.ToSingle(vecotr[0]);
			float y= (float)System.Convert.ToSingle(vecotr[1]);
			float z= (float)System.Convert.ToSingle(vecotr[2]);
			Vector3 pos = new Vector3(x,y,z) ;
			spawnPoint.position=pos;
			Debug.Log(pos);
		}	
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
