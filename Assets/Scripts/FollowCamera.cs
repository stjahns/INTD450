using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {
	
	
	// Set target
	public Transform target;
	private int zoom = 20;
	private int normal = 60;
	private float smooth =5;
	private bool isZoomed = false;
	
	void  Update (){


		transform.position = new Vector3(
			target.position.x, 
			target.position.y, 
			transform.position.z 
				);
		if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			isZoomed = !isZoomed; 
		} else {
			if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
				isZoomed = isZoomed; 
			}
		}
		
		if(isZoomed == true){
			camera.fieldOfView = Mathf.Lerp(camera.fieldOfView,zoom,Time.deltaTime*smooth);
		}
		else{
			camera.fieldOfView = Mathf.Lerp(camera.fieldOfView,normal,Time.deltaTime*smooth);
		}

	} 
	
} 
