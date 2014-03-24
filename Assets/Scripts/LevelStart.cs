using UnityEngine;
using System.Collections;
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
    void Start()
    {
        SpawnPlayer();
    }

    //
    // 
    //
    GameObject load_object(string prefab_name, string spawner_tag, Vector3 postion, Quaternion rotation_data)
    {
        GameObject pNewObject = null;
        try
        {

            ////  prefab_name = prefab_name.Split('(')[0];
            GameObject pPrefab = Resources.Load(prefab_name) as GameObject;
            if (spawner_tag != null)
            {
               //// Debug.Log("Found");
                prefab_name += spawner_tag;
            }
            
            if (pPrefab != null)
            {
                pNewObject = (GameObject)GameObject.Instantiate(pPrefab, postion, rotation_data);
                pNewObject.name = prefab_name;
            }
        }
        catch
        {
            Debug.Log("Cant load");
        }
        return pNewObject;
    }
    //
    // Instantiates the playerPrefab at the spawnPoint
    //
    public void SpawnPlayer()
    {

        Save_Load load = new Save_Load();
        load.player_name = "player";
        var data = load.file_load();
      ////  Debug.Log(data);
        string checkpoint = null;
        string player_pos = "";
        int level = 0;

        checkpoint = data["array"][1]["checkpoint"];
      ////  Debug.Log("FLAGGGG" + checkpoint);
        player_pos = data["array"][1]["player_pos"];
        level = System.Convert.ToInt32(data["array"][1]["Level"]);
       //// Debug.Log("Level" + Application.loadedLevel + ":" + level);
        if (checkpoint != null && level == Application.loadedLevel)
        {
            RobotComponent[] robot_obj = FindObjectsOfType(typeof(RobotComponent)) as RobotComponent[];
            foreach (RobotComponent comp in robot_obj)
            {
                Destroy(comp.gameObject);
            }

            string[] checkpoint_data = checkpoint.Split('/');
            foreach (string component_data in checkpoint_data)
            {
                string spawner_tag = null;
                if (component_data != "")
                {
                    ///Debug.Log("S" +s);
                    string[] pos = component_data.Split(':');
                    ///int id = 0;
                    ///Debug.Log(pos);

                    if (pos[0] != "" && pos[0] != "HED-I(Clone)")
                    {
                        string [] object_name_array = pos[0].Split('('); //To check if it contains Clone
                        string object_name = object_name_array[0];
                        string spawner_name = null;
                        if (object_name_array.Length > 1)
                        {
                            spawner_name = object_name_array[1].Split(')')[0];
                            spawner_tag = "(" + spawner_name + ")";
                            
                        }
                        checkpoint = pos[2].Replace('(', ' ');
                        checkpoint = checkpoint.Replace(')', ' ');
                        string[] postion = checkpoint.Split(',');
                        float x = (float)System.Convert.ToSingle(postion[0]);
                        float y = (float)System.Convert.ToSingle(postion[1]) + 2;
                        float z = (float)System.Convert.ToSingle(postion[2]);
                        Vector3 postion_data = new Vector3(x, y, z);

                        checkpoint = pos[1].Replace('(', ' ');
                        checkpoint = checkpoint.Replace(')', ' ');

                        string[] rotation = checkpoint.Split(',');
                        float x1 = (float)System.Convert.ToSingle(rotation[0]);
                        float y1 = (float)System.Convert.ToSingle(rotation[1]);
                        float z1 = (float)System.Convert.ToSingle(rotation[2]);
                        float f = (float)System.Convert.ToSingle(rotation[3]);
                        Quaternion rotation_data = new Quaternion(x1, y1, z1, f);
                        GameObject vv = load_object(object_name, spawner_tag, postion_data, rotation_data);
                        if (spawner_name != null )
                        {
                            ObjectSpawner objectspawner_script;
                            ////Debug.Log(spawner_name);
                            GameObject spawner = GameObject.Find(spawner_name);
                            if (spawner != null)
                            {
                                objectspawner_script = spawner.gameObject.GetComponent<ObjectSpawner>();
                                objectspawner_script.spawnedObjects.Add(vv);
                            }
                        }
                        

                    }

                    if (player_pos != null && pos[0] == "HED-I(Clone)")
                    {
                        ///Debug.Log(player_pos);
                        player_pos = player_pos.Replace('(', ' ');
                        player_pos = player_pos.Replace(')', ' ');
                        string[] vector_player = player_pos.Split(',');
                        float player_x = (float)System.Convert.ToSingle(vector_player[0]) - 2;
                        float player_y = (float)System.Convert.ToSingle(vector_player[1]);
                        float player_z = (float)System.Convert.ToSingle(vector_player[2]);
                        Vector3 player_postion = new Vector3(player_x, player_y, player_z);
                        spawnPoint.position = player_postion;
                    }
                }
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
