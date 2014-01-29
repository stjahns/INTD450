using UnityEngine;
using System.Collections;

public class TriggerSave : MonoBehaviour {

	// Use this for initialization
	void OnTriggerEnter2D (Collider2D other){
	///	other.attachedRigidbody
		int level = Application.loadedLevel;
		Save_Load save = new Save_Load ();
		save.player_name="player";
		Vector3 playerPos = other.attachedRigidbody.transform.position;
        
		save.add_checkpoint(level,playerPos);
		Debug.Log("Save Now"+playerPos);

	}
	void OnTriggerExit2D (Collider2D other){
		
		Save_Load save = new Save_Load ();
		save.player_name="player";
		Debug.Log(save.file_load ());
		Debug.Log("Save Now");
		
	}
}
