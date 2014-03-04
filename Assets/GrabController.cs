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
		foreach (var hit in Physics2D.LinecastAll(grabOrigin.position, grabCheck.position,
					layerMask))
		{
			if (hit.rigidbody != rigidbody2D) // if it's not ourself...
			{
				grabJoint = gameObject.AddComponent<DistanceJoint2D>();
				grabJoint.connectedBody = hit.rigidbody;
				grabJoint.connectedAnchor = hit.point - hit.rigidbody.transform.position.XY();
				grabJoint.anchor = grabOrigin.position - rigidbody2D.transform.position;
				grabJoint.distance = 0.0f;
				break;
			}
		}
	}
}
