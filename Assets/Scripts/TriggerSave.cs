using UnityEngine;
using System.Collections;

public class TriggerSave : MonoBehaviour {

	// Use this for initialization
	void OnTriggerEnter2D (Collider2D other){
        RobotComponent[] obj = FindObjectsOfType(typeof(RobotComponent)) as RobotComponent[];
        string checkpoint = "";
		int level = Application.loadedLevel;
		Save_Load save = new Save_Load ();
		save.player_name="player";
		Vector3 playerPos = other.attachedRigidbody.transform.position;
        foreach (RobotComponent gam in obj)
        {
            checkpoint += gam.GetInstanceID() + ":" + gam.transform.rotation.ToString() + ":" + gam.transform.position.ToString() + "/";
        }
        save.add_checkpoint(level, checkpoint);
        Debug.Log("Save Now" + checkpoint);
	}
	void OnTriggerExit2D (Collider2D other){
		
		Save_Load save = new Save_Load ();
		save.player_name="player";
		Debug.Log(save.file_load ());
		Debug.Log("Save Now");
		
	}
}
