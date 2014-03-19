using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Save_loadV2 : MonoBehaviour {

	// Use this for initialization
    
	void Start () {
        RobotComponent[] obj = FindObjectsOfType(typeof(RobotComponent)) as RobotComponent[];
        Dictionary<string, string> game_object_components= new Dictionary<string, string>();
        ////Dictionary<string, string> game_object_components = new Dictionary<string, string>();
        string Data="";
        foreach (RobotComponent test in obj)
        {
            //Instance ID is the Key
            //The Postion 
            Data +="/"+test.GetInstanceID()+":"+ test.transform.rotation.ToString() + ":" + test.transform.position.ToString()+"/";
    
           /// Debug.Log(test.GetInstanceID());
           //// Debug.Log(Data);
        }

        
        Debug.Log(Data);
        string[] spli = Data.Split('/');
        foreach(string s in spli)
        {
            Debug.Log(s);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
   
}
