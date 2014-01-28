using UnityEngine;
using System.Collections;
using SimpleJSON;

public class levelmenu : MonoBehaviour {
	public Save_Load load;
	public string player_name="";
	void OnGUI  () 
	{
		load = new Save_Load ();
		load.player_name = player_name;
		var data =load.file_load ();
		Debug.Log (data);
		int level = System.Convert.ToInt32(data["array"][1]["Level"]);
		Debug.Log ("Level "+ data["array"][1]["Level"]);
		Debug.Log ("Scroe "+ data["array"][1]["Score"]);

		if (GUI.Button (new Rect (350, 100, 100, 30), "Level 1")) 
		{
			if  (level+1 >= 1)
			{ 
				Application.LoadLevel(2);
			}else
			{
				Debug.Log("Cant advance ");
			}
			
		}
			
		if (GUI.Button (new Rect (350, 200, 100, 30), "Level 2")) 
		{
			if  (level+1 >= 2)
			{ 
				Debug.Log("Can advance ");
			}else
			{	
				Debug.Log("Cant advance ");
			}
		}
	
	if (GUI.Button (new Rect (350, 300, 100, 30), "Level 3")) 
		{
		
		
		}


	}
}

