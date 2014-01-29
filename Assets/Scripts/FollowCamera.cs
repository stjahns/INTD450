using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {
	
	
	// Set target
	public Transform target;
	public int zoom = 20;
	public int zoom_size=5;
	public int normal = 60;
	public float smooth =5;
	private bool isZoomed = false;
	
	void  Update (){


		transform.position = new Vector3(
			target.position.x, 
			target.position.y, 
			transform.position.z 
				);

		if (Input.GetKeyDown(KeyCode.M)) {
			Debug.Log("True");

			zoom+=zoom_size;


		} 
		if (Input.GetKeyDown(KeyCode.N)) {
				Debug.Log("False");

			zoom-=zoom_size;

			}

		camera.orthographicSize= Mathf.Lerp(camera.orthographicSize,zoom,Time.deltaTime*smooth);
		if(isZoomed == true){


		}
		else{

		}

	} 
	
} 
