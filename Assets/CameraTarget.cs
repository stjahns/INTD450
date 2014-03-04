using UnityEngine;
using System.Collections;

public class CameraTarget : MonoBehaviour
{
	public Transform oldTarget = null;

	[InputSocket]
	public void AcquireCamera()
	{
		FollowCamera camera = Camera.mainCamera.GetComponent<FollowCamera>();
		if (camera)
		{
			oldTarget = camera.target;
			camera.target = this.transform;
		}
	}

	[InputSocket]
	public void ReleaseCamera()
	{
		FollowCamera camera = Camera.mainCamera.GetComponent<FollowCamera>();
		if (camera && oldTarget)
		{
			camera.target = oldTarget;
		}
	}

	public void OnDrawGizmos()
	{
	}
}
