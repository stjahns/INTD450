using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Save_loadV2 : MonoBehaviour {

	// Use this for initialization
    
	void Start () {
        RobotComponent[] obj = FindObjectsOfType(typeof(RobotComponent)) as RobotComponent[];
        ////Dictionary<string, string> game_object_components;
        ////Dictionary<string, string> game_object_components = new Dictionary<string, string>();
        var list = new Dictionary<string, string>();


        foreach (RobotComponent test in obj)
        {
            //Instance ID is the Key
            //The Postion 
            string Data = test.transform.rotation.ToString() + ":" + test.transform.position.ToString();
            list[test.GetInstanceID().ToString()] = Data;
            
            Debug.Log(test.GetInstanceID());
            Debug.Log(Data);
        }
        
        

        ///Debug.Log(d);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
   
}
