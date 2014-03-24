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
        string boxes = "";
        int level = 0;

        checkpoint = data["array"][1]["checkpoint"];
        boxes = data["array"][1]["boxes"];
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
                        Vector3 postion_data = create_vector3(pos[2], false);
                        Quaternion rotation_data = create_Quaternion(pos[1]);
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

                        Vector3 player_postion = create_vector3(player_pos, true);
                        spawnPoint.position = player_postion;
                    }
                }
            }
            if (boxes != null)
            {
                BoxComponent[] boxes_Data = FindObjectsOfType(typeof(BoxComponent)) as BoxComponent[];
                foreach (BoxComponent comp in boxes_Data)
                {
                    Destroy(comp.gameObject);
                }
                string[] box_data = boxes.Split('/');
                foreach (string component_data in box_data)
                {
                    string[] pos = component_data.Split(':');
                    Vector3 postion_data = create_vector3(pos[2], false);
                    Quaternion rotation_data = create_Quaternion(pos[1]);
                    GameObject vv = load_object("Boxes",null, postion_data, rotation_data);

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

    private Vector3 create_vector3(string data,bool player)
    {
        Vector3 return_data;
        data = data.Replace('(', ' ');
        data = data.Replace(')', ' ');
        string[] postion = data.Split(',');
        float x;
        float y;
        float z;
        if (player)
        {
            x = (float)System.Convert.ToSingle(postion[0]) - 2;
            y = (float)System.Convert.ToSingle(postion[1]);
        }
        else
        {
            x = (float)System.Convert.ToSingle(postion[0]);
            y = (float)System.Convert.ToSingle(postion[1]) + 2;
        }

        z = (float)System.Convert.ToSingle(postion[2]);

        return_data = new Vector3(x, y, z);

        return return_data;

    }

    private Quaternion create_Quaternion(string data)
    {
        Quaternion return_data;
        data = data.Replace('(', ' ');
        data = data.Replace(')', ' ');
        string[] rotation = data.Split(',');
        float x1 = (float)System.Convert.ToSingle(rotation[0]);
        float y1 = (float)System.Convert.ToSingle(rotation[1]);
        float z1 = (float)System.Convert.ToSingle(rotation[2]);
        float f = (float)System.Convert.ToSingle(rotation[3]);
        return_data = new Quaternion(x1, y1, z1, f);
        return return_data;
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
