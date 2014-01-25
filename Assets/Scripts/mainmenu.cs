using UnityEngine;
using System.Collections;

public class mainmenu : MonoBehaviour {

	// Use this for initialization
	void OnGUI  () {

		if (GUI.Button (new Rect (350,100, 100, 30), "Start Game")) {

			Application.LoadLevel(1);


		}

		if (GUI.Button (new Rect (350, 200, 100, 30), "Option")) {


		}

		if (GUI.Button (new Rect (350, 300, 100, 30), "Exit Game")) {


		}

	
	}
}
