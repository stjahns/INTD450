using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour
{
	
	public Transform target;

	public float viewportHeight = 5;

	public float zoomStepSize = 5;

	public float zoomSmoothing = 5;

	public float maxViewportHeight = 15;

	public float minViewportHeight = 1;
	
	void Update ()
	{

		transform.position = new Vector3(
				target.position.x, 
				target.position.y, 
				transform.position.z);

		if (Input.GetKeyDown(KeyCode.M))
		{
			if (viewportHeight + zoomStepSize <= maxViewportHeight)
			{
				viewportHeight += zoomStepSize;
			}

		} 

		if (Input.GetKeyDown(KeyCode.N))
		{
			if (viewportHeight - zoomStepSize >= minViewportHeight)
			{
				viewportHeight -= zoomStepSize;
			}
		}

		camera.orthographicSize= Mathf.Lerp(
				camera.orthographicSize,
				viewportHeight,
				Time.deltaTime * zoomSmoothing);

	} 

} 
