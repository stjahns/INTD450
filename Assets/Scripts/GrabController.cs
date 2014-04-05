using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrabController : MonoBehaviour
{
	public Sprite openSprite;
	public Sprite closedSprite;

	public SpriteRenderer spriteRenderer;

	public List<string> grabbableLayers;

	public Transform grabOrigin;
	public Transform grabCheck;

	private Rigidbody2D grabbedBody;
	private DistanceJoint2D grabJoint;

	void Start()
	{
		spriteRenderer.sprite = openSprite;
		grabJoint = null;
	}

	[InputSocket]
	public void Open()
	{
		spriteRenderer.sprite = openSprite;

		if (grabJoint != null)
		{
			Destroy(grabJoint);
			grabJoint = null;
		}
	}

	[InputSocket]
	public void Close()
	{
		spriteRenderer.sprite = closedSprite;

		int layerMask = 0;
		grabbableLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));

		// Check if any grabbleble colliders below
		foreach (var collider in Physics2D.OverlapCircleAll(grabCheck.position, 0.2f, layerMask))
		{
			if (collider.rigidbody2D != rigidbody2D)
			{
				grabJoint = gameObject.AddComponent<DistanceJoint2D>();
				grabJoint.connectedBody = collider.rigidbody2D;
				grabJoint.anchor = grabOrigin.position - rigidbody2D.transform.position;
				grabJoint.distance = 0.1f;
				break;
			}
		}
	}
}
