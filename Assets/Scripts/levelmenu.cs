using UnityEngine;
using System.Collections;
using SimpleJSON;

public class levelmenu : MonoBehaviour {

	public Save_Load load ;
	public string player_name=" ";
	string show()
	{
		Debug.Log (load.file_load());
		string s = load.file_load ();

		return s;
	}
	void OnGUI  () 
	{

		var data =JSONNode.LoadFromBase64(show ());


				int level = System.Convert.ToInt32(data["array"]["1"]["level"])+1;

						if (GUI.Button (new Rect (350, 100, 100, 30), "Level 1")) {

							if (level <= 1){ 
							Application.LoadLevel (2);
							}
			
			
						}
		
						if (GUI.Button (new Rect (350, 200, 100, 30), "Level 2")) {
			
			
						}
		
						if (GUI.Button (new Rect (350, 300, 100, 30), "Level 3")) {
			
			
						}


}
}
