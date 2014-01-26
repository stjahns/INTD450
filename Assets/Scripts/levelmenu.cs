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


				int level = System.Convert.ToInt32(data["array"]["1"]["level"])+1;

						if (GUI.Button (new Rect (350, 100, 100, 30), "Level 1")) {

							
							if  (level >= 1){ 
				Application.LoadLevel(2);
								
							}else{	
								Debug.Log("Cant advance ");
							}
							
						}
			
			if (GUI.Button (new Rect (350, 200, 100, 30), "Level 2")) {
								if  (level >= 2){ 
									Debug.Log("Can advance ");
								}else{	
				Debug.Log("Cant advance ");
									}
		}
		
		if (GUI.Button (new Rect (350, 300, 100, 30), "Level 3")) {
			
			
						}


}
}

