using UnityEngine;
using System.Collections;


public class TriggerSave : MonoBehaviour {
    ///public static PlayerBehavior player;

	// Use this for initialization
	void OnTriggerEnter2D (Collider2D other){
        string checkpoint = "";
        ///Debug.Log(other.gameObject.name);
        if (other.gameObject.name == "HED-I(Clone)")
        {
            PlayerBehavior player = PlayerBehavior.Player;
            RobotComponent[] obj = FindObjectsOfType(typeof(RobotComponent)) as RobotComponent[];
            Debug.Log("Length"+obj.Length);
		    int level = Application.loadedLevel;
		    Save_Load save = new Save_Load ();
		    save.player_name="player";
		    Vector3 playerPos = other.attachedRigidbody.transform.position;
            foreach (RobotComponent gam in obj)
            {
              ////  Data += "/" + test.name + "/" + test.GetInstanceID() + ":" + test.transform.rotation.ToString() + ":" + test.transform.position.ToString() + "/";
                checkpoint += gam.GetInstanceID() + ":" + gam.transform.rotation.ToString() + ":" + gam.transform.position.ToString() + ":" + gam.name+"/";
            }
            Debug.Log(checkpoint);
            if (player != null)
            {
                save.add_checkpoint(level, checkpoint, player.transform.position);
                Debug.Log("Save Now" + checkpoint);
            }
        }
	}
	void OnTriggerExit2D (Collider2D other){
		
		Save_Load save = new Save_Load ();
		save.player_name="player";
		Debug.Log(save.file_load ());
		Debug.Log("Save Now");
		
	}
}
