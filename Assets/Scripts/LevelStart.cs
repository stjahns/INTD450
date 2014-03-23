using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class LevelStart : MonoBehaviour
{

	public GameObject playerPrefab;
	public PlayerBehavior player;
	public Transform spawnPoint;

	public delegate void PlayerSpawnedHandler(GameObject spawner, GameObject spawnedPlayer);
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
        string player_pos="";
		int level = 0;
		flag = data ["array"][1]["checkpoint"];
        Debug.Log("FLAGGGG" + flag);
        player_pos =  data["array"][1]["player_pos"];
		level = System.Convert.ToInt32(data ["array"][1]["level"]);
       
		if (flag != "Null" && level == Application.loadedLevel) 
		{
            Dictionary<int, GameObject> myDict=new Dictionary<int, GameObject>();
            RobotComponent[] robot_obj = FindObjectsOfType(typeof(RobotComponent)) as RobotComponent[];
            string[] spli = flag.Split('/');
            Debug.Log(spli);
            foreach(RobotComponent comp in robot_obj)
            {
                myDict.Add((int)comp.GetInstanceID(), comp.gameObject);
            }
           //// Debug.Log("Hello");
            foreach (string s in spli)
            {
                if (s != "")
                {
                Debug.Log("S" +s);
                string[] pos = s.Split(':');
                int id = 0;
                Debug.Log("again"+pos[0]);
                Debug.Log("again"+pos[1]);
                Debug.Log("again"+pos[2]);
                ////Debug.Log("again"+pos[3]); 
                Debug.Log(pos);
               
                    if (pos[2] != "")
                    {
                        Debug.Log(pos[0]);
                        id = (int)System.Convert.ToInt64(pos[0]);
                        if (id > 0)
                        {
                            Debug.Log(id);
                            if (myDict.ContainsKey(id))
                            {
                                Debug.Log("Contains" + pos[2]);
                                GameObject objec = myDict[id];
                                if (objec != null)
                                {
                                    flag = pos[2].Replace('(', ' ');
                                    flag = flag.Replace(')', ' ');
                                    string[] vecotr = flag.Split(',');
                                    float x = (float)System.Convert.ToSingle(vecotr[0]);
                                    float y = (float)System.Convert.ToSingle(vecotr[1]);
                                    float z = (float)System.Convert.ToSingle(vecotr[2]);
                                    Vector3 postion_data = new Vector3(x, y, z);
                                    Debug.Log("Con"+postion_data);
                                    Debug.Log(postion_data);
                                    objec.transform.position = postion_data;

                                }
                            }
                        }
                    }

                    if (player_pos != null && pos[3] == "HED-I(Clone)")
                    {
                        Debug.Log(player_pos);
                        player_pos = player_pos.Replace('(', ' ');
                        player_pos = player_pos.Replace(')', ' ');
                        string[] vector_player = player_pos.Split(',');
                        float player_x = (float)System.Convert.ToSingle(vector_player[0]);
                        float player_y = (float)System.Convert.ToSingle(vector_player[1]);
                        float player_z = (float)System.Convert.ToSingle(vector_player[2]);
                        Vector3 player_postion = new Vector3(player_x, player_y, player_z);
                        spawnPoint.position = player_postion;
                    }
                }
                ////ID.GetInstanceID(InstanceID);


            }
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
