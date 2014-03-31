using UnityEngine;
using System.Collections;

public class FloatingComponent : MonoBehaviour
{
	[SerializeField]
	private float _floatForce;

	[SerializeField]
	private Vector2 _floatCenter;

	[SerializeField]
	private float _submergedDrag;

	[SerializeField]
	private float _submergedAngularDrag;

	[SerializeField]
	private float _floatTorque;

	[SerializeField]
	private Vector2 _floatCheckArea = new Vector2(0.1f, 0.1f);

	void FixedUpdate()
	{
		int layerMask = 1 << LayerMask.NameToLayer("Water");
		Vector2 position = transform.position;
		Collider2D water = Physics2D.OverlapArea(position + _floatCenter + _floatCheckArea,
				position + _floatCenter - _floatCheckArea, layerMask);

		if (water)
		{
			var waterBox = water.GetComponent<BoxCollider2D>();
			float waterTop = waterBox.transform.position.y 
				+ waterBox.center.y 
				+ waterBox.size.y / 2;

			// if checkArea fully submerged, full force
			// if checkArea fully above, no force
			float floatCheckBottom = position.y + _floatCenter.y - _floatCheckArea.y;
			float forceScale = (waterTop - floatCheckBottom) / (_floatCheckArea.y * 2);
			rigidbody2D.AddForce(Vector3.up * _floatForce * forceScale);

			// Add drag from water
			rigidbody2D.drag = _submergedDrag;

			// Keep object upright!
			var angleError = Mathf.DeltaAngle(transform.eulerAngles.z, 0);
			rigidbody2D.AddTorque(_floatTorque * angleError / 180);
			rigidbody2D.angularDrag = _submergedAngularDrag;
		}
		else
		{
			rigidbody2D.angularDrag = 0;
			rigidbody2D.drag = 0;
		}
	}

}
