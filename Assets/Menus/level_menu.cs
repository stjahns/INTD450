using UnityEngine;
using System.Collections;
using SimpleJSON;

public class level_menu : MonoBehaviour {

	// Use this for initialization
	public int level_sprite;
	public Save_Load load;
	private string player_name="player";
	private int current_level;
	private int player_level;
    private string[] current_object_name;

	void Start () {

		load = new Save_Load ();
		load.player_name = player_name;
		var data =load.file_load ();
		Debug.Log (data);
        ////string [] current_object_name;
        current_object_name = transform.name.Split(' ');
        Debug.Log(current_object_name[0]);
        current_level = System.Convert.ToInt32 (current_object_name[1]);
        
		 player_level = System.Convert.ToInt32(data["array"][1]["Level"]);
         if (player_level == 0)
         {
             player_level += 1;
         }
        ///Debug.Log ("Level "+ data["array"][1]["Level"]);
		///Debug.Log ("Scroe "+ data["array"][1]["Score"]);
		///Debug (System.Convert.ToInt32(gameObject.name));
		 ////current_level = System.Convert.ToInt32 (transform.name);
		Debug.Log ("Player level"+player_level);
		Debug.Log (transform.name+"Current level"+current_level);
        if (current_object_name[0] == "X")
        {
            if (player_level >= current_level)
            {
                Destroy(this);

            }
        }
	
	}
    void OnMouseUp()
    {
        if (current_object_name[0] == "level")
        {
            if (player_level >= current_level)
            {
                Application.LoadLevel(current_level);

            }
            else
            {
                Debug.Log("Cant advance ");
            }
        }
    }

}
