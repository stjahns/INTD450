using UnityEngine;
using System.Collections;

public class CameraTarget : MonoBehaviour
{
	public float targetViewportHeight;
	public bool allowZoom;
	public float transitionTime = 0.5f;

	public float maxViewportHeight = 15;
	public float minViewportHeight = 1;
	public float zoomStepSize = 5;

	private float pushCount = 0;

	[InputSocket]
	virtual public void AcquireCamera()
	{
		FollowCamera camera = Camera.main.GetComponent<FollowCamera>();
		if (camera)
		{
			camera.PushTarget(this, transitionTime);
			pushCount++;
		}
	}

	[InputSocket]
	virtual public void ReleaseCamera()
	{
		if (pushCount > 0)
		{
			FollowCamera camera = Camera.main.GetComponent<FollowCamera>();
			if (camera)
			{
				camera.PopTarget(this, transitionTime);
				pushCount--;
			}
		}
	}

	public void OnDrawGizmos()
	{
	}
}
