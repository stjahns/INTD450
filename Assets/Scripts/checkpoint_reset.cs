using UnityEngine;
using System.Collections;

public class checkpoint_reset : MonoBehaviour {

	// Use this for initialization
	void Start () {
        int level = Application.loadedLevel;
        Save_Load save = new Save_Load();
        save.player_name = "player";
        Vector3 empty=new Vector3() ;
        save.add_checkpoint(level, "Null", "Null",empty);	
	}
	
	
}
