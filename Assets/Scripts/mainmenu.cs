using UnityEngine;
using System.Collections;

public class mainmenu : MonoBehaviour {

	// Use this for initialization
	void OnGUI  () {

		if (GUI.Button (new Rect (350,100, 150, 30), "Start New Game")) {
			Save_Load load = new Save_Load();
			load.player_name="player";
			load.create_new();
			Application.LoadLevel(1);


		}

		if (GUI.Button (new Rect (350, 200, 100, 30), "Resume Game")) {
			Save_Load load = new Save_Load();
			load.player_name="player";
			var data=load.file_load();
			Debug.Log(data);
			int level=System.Convert.ToInt32(data["array"][1]["Level"]);
			if (level==0){
				level+=2;
			}
			Application.LoadLevel(level);
		}

		if (GUI.Button (new Rect (350, 300, 100, 30), "Exit Game")) {

			Application.Quit();
		}

	
	}
}
