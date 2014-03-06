using UnityEngine;
using System.Collections;

public class CameraTarget : MonoBehaviour
{
	public float targetViewportHeight;
	public bool allowZoom;

	private float pushCount = 0;

	[InputSocket]
	public void AcquireCamera()
	{
		FollowCamera camera = Camera.main.GetComponent<FollowCamera>();
		if (camera)
		{
			camera.PushTarget(this, 0.5f);
			pushCount++;
		}
	}

	[InputSocket]
	public void ReleaseCamera()
	{
		if (pushCount > 0)
		{
			FollowCamera camera = Camera.main.GetComponent<FollowCamera>();
			if (camera)
			{
				camera.PopTarget(this, 0.5f);
				pushCount--;
			}
		}
	}

	public void OnDrawGizmos()
	{
	}
}
