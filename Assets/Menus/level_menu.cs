using UnityEngine;
using System.Collections;
using SimpleJSON;

public class level_menu : MonoBehaviour {

	// Use this for initialization
	public Sprite level_sprite;
	public Save_Load load;
	private string player_name="player";
	private int current_level;
	private int player_level;

	void Start () {
		SpriteRenderer render = gameObject.GetComponent <SpriteRenderer>();
		load = new Save_Load ();
		load.player_name = player_name;
		var data =load.file_load ();
		Debug.Log (data);

		 player_level = System.Convert.ToInt32(data["array"][1]["Level"]);
		///Debug.Log ("Level "+ data["array"][1]["Level"]);
		///Debug.Log ("Scroe "+ data["array"][1]["Score"]);
		///Debug (System.Convert.ToInt32(gameObject.name));
		 current_level = System.Convert.ToInt32 (transform.name);
		Debug.Log ("Player level"+player_level);
		Debug.Log (transform.name+"Current level"+current_level);
		if  (player_level  >= current_level)
		{ 
			render.sprite = level_sprite;

		}else
		{
			Debug.Log("Cant advance ");
		}
	
	}
	void  OnMouseUp() {
		if  (player_level  >= current_level)
		{ 
			Application.LoadLevel(current_level);
			
		}else
		{
			Debug.Log("Cant advance ");
		}


		}

}
