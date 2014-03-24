using UnityEngine;
using System.Collections;

public class instance_id : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Debug.Log(gameObject.GetInstanceID());
	}
	
	// Update is called once per frame

}
