using UnityEngine;
using System.Collections;

// Special CameraTarget that allows free movement with WSAD keys,
// constrained within a box
public class FreeCamera : CameraTarget
{
	public float moveSpeed = 1f;

	public Transform topLeft;
	public Transform bottomRight;
	
	private Vector3 startPosition;

	void Start()
	{
		startPosition = transform.position;
		topLeft.transform.parent = null;
		bottomRight.transform.parent = null;
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.W) && transform.position.y < topLeft.position.y)
		{
			transform.Translate(Vector3.up * moveSpeed);
		}

		if (Input.GetKey(KeyCode.S) && transform.position.y > bottomRight.position.y)
		{
			transform.Translate(Vector3.down * moveSpeed);
		}

		if (Input.GetKey(KeyCode.A) && transform.position.x > topLeft.position.x)
		{
			transform.Translate(Vector3.left * moveSpeed);
		}

		if (Input.GetKey(KeyCode.D) && transform.position.x < bottomRight.position.x)
		{
			transform.Translate(Vector3.right * moveSpeed);
		}
	}

	override public void ReleaseCamera()
	{
		base.ReleaseCamera();

		// Return to original position
		transform.position = startPosition;
	}
}
