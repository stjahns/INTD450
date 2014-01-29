using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {
	
	
	// Set target
	public Transform target;
	[HideInInspector]
	public int zoom = 5;
	[HideInInspector]
	public int zoom_size=5;
	[HideInInspector]
	public int normal = 60;
	[HideInInspector]
	public float smooth =5;
	
	void  Update (){


		transform.position = new Vector3(
			target.position.x, 
			target.position.y, 
			transform.position.z 
				);

		if (Input.GetKeyDown(KeyCode.M)) {
			Debug.Log("True");
			if (zoom <15)
			{
			zoom+=zoom_size;
			}

		} 
		if (Input.GetKeyDown(KeyCode.N)) {
				Debug.Log("False");
			if (zoom > 0)
			{
			zoom-=zoom_size;
			}
			}

		camera.orthographicSize= Mathf.Lerp(camera.orthographicSize,zoom,Time.deltaTime*smooth);

	} 
	
} 
