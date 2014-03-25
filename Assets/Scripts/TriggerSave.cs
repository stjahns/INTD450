using UnityEngine;
using System.Collections;


public class TriggerSave : MonoBehaviour {
    ///public static PlayerBehavior player;

	// Use this for initialization
	void OnTriggerEnter2D (Collider2D other){
        string checkpoint = "";
        string boxes_checkpoint = "";
        ///Debug.Log(other.gameObject.name);
        if (other.gameObject.name == "HED-I(Clone)")
        {
            PlayerBehavior player = PlayerBehavior.Player;
            RobotComponent[] obj = FindObjectsOfType(typeof(RobotComponent)) as RobotComponent[];
            BoxComponent[] level_boxes = FindObjectsOfType(typeof(BoxComponent)) as BoxComponent[];        
            
            ///   Debug.Log("Length"+obj.Length);
		    int level = Application.loadedLevel;
		    Save_Load save = new Save_Load ();
		    save.player_name="player";
		  ///  Vector3 playerPos = other.attachedRigidbody.transform.position;
            foreach (RobotComponent gam in obj)
            {
              ////  Data += "/" + test.name + "/" + test.GetInstanceID() + ":" + test.transform.rotation.ToString() + ":" + test.transform.position.ToString() + "/";
                checkpoint += gam.name + ":" + gam.transform.rotation.ToString() + ":" + gam.transform.position.ToString() + "/";
            }
            Debug.Log(level_boxes.Length + "boxle");
           if (level_boxes.Length >=1)
           {
               foreach (BoxComponent box in level_boxes)
               {
                   Debug.Log(box.name);
                   boxes_checkpoint += "Box:" + box.transform.rotation.ToString() + ":" + box.transform.position.ToString() + "/";
               }
           }
            if (player != null)
            {
           ///     Debug.Log(level);
                save.add_checkpoint(level, checkpoint, boxes_checkpoint, player.transform.position);
             ///   Debug.Log("Save Now" + checkpoint);
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
