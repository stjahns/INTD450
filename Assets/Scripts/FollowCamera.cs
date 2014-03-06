using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowCamera : MonoBehaviour
{

	public float viewportHeight = 5;

	public float zoomStepSize = 5;

	public float zoomSmoothing = 5;

	public float maxViewportHeight = 15;

	public float minViewportHeight = 1;

	private bool inTransition;
	private float targetTransitionTime = 0;
	private float targetTransitionTimer = 0;

	private List<CameraTarget> targetStack = new List<CameraTarget>();

	void Start ()
	{
		inTransition = false;
	}
	
	void Update ()
	{
		if (targetStack.Count > 0)
		{
			CameraTarget currentTarget = targetStack[0];

			Vector3 targetPosition = new Vector3(
					currentTarget.transform.position.x, 
					currentTarget.transform.position.y, 
					transform.position.z);

			if (inTransition)
			{
				targetTransitionTimer += Time.deltaTime;

				if (targetTransitionTimer > targetTransitionTime)
				{
					inTransition = false;
					targetTransitionTimer = targetTransitionTime;
				}

				transform.position = Vector3.Lerp(transform.position, targetPosition,
						targetTransitionTimer / targetTransitionTime);
			}
			else
			{
				transform.position = targetPosition;
			}

			if (currentTarget.allowZoom)
			{
				// Zoom in
				if (Input.GetKeyDown(KeyCode.M))
				{
					if (viewportHeight + zoomStepSize <= maxViewportHeight)
					{
						viewportHeight += zoomStepSize;
					}

				} 

				// Zoom out
				if (Input.GetKeyDown(KeyCode.N))
				{
					if (viewportHeight - zoomStepSize >= minViewportHeight)
					{
						viewportHeight -= zoomStepSize;
					}
				}
			}

			camera.orthographicSize= Mathf.Lerp(
					camera.orthographicSize,
					viewportHeight,
					Time.deltaTime * zoomSmoothing);
		}
	} 

	// Add target to top of stack (first element in list)
	public void PushTarget(CameraTarget newTarget, float transitionTime = 0)
	{
		targetStack.Insert(0, newTarget);
		viewportHeight = newTarget.targetViewportHeight;

		targetTransitionTime = transitionTime;
		targetTransitionTimer = 0;
		inTransition = true;
	}

	// Remove first instance of given target from stack,
	// or remove top target of stack
	public void PopTarget(CameraTarget oldTarget = null, float transitionTime = 0)
	{
		if (targetStack.Count > 0)
		{
			if (oldTarget != null)
			{
				targetStack.Remove(oldTarget);
			}
			else
			{
				targetStack.RemoveAt(0);
			}
		}

		if (targetStack.Count > 0)
		{
			viewportHeight = targetStack[0].targetViewportHeight;
		}

		inTransition = true;
		targetTransitionTime = transitionTime;
		targetTransitionTimer = 0;
	}
} 
