using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {
	
	
	// Set target
	public Transform target;
	
	void  Update (){

		if (Input.GetKey (KeyCode.Space)) {
						camera.fieldOfView = 60;
				} else {
			camera.fieldOfView = 60;
				}
		transform.position = new Vector3(
			target.position.x, 
			target.position.y, 
			transform.position.z 
				);

	} 
	
} 
