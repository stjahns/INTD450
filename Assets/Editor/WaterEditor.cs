using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

[CustomEditor(typeof(Water), true)]
public class WaterEditor : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Adjust Bounds"))
		{
			AdjustBounds();
		}

		base.OnInspectorGUI();
	}

	void AdjustBounds()
	{
		Water water = target as Water;

		float width = Mathf.Abs(water.transform.position.x - water.leftBound.transform.position.x);
		float height = Mathf.Abs(water.transform.position.y - water.bottomBound.transform.position.y);
		// ADJUST SPRITES

		// Place front at origin
		water.front.transform.localPosition = Vector3.zero;

		// front sprite is 0.1 x 0.1, stretch to fit
		water.front.transform.localScale = new Vector3(width * 10f, height * 10f, 1f);
		
		// Place endFront at origin
		water.endFront.transform.localPosition = Vector3.zero;
		water.endFront.transform.localScale = Vector3.one;

		// Place endBack at origin + slopeHeight
		water.endBack.transform.localPosition = Vector3.zero 
			+ Vector3.up * water.slopeHeight;
		water.endBack.transform.localScale = Vector3.one;

		// Place topBack at origin + slopeHeight + slopeWidth
		water.topBack.transform.localPosition = Vector3.zero
			+ Vector3.left * water.slopeWidth
			+ Vector3.up * water.slopeHeight;
		water.topBack.transform.localScale = new Vector3(width - water.slopeWidth, 1f, 1f);

		// Place topFront at origin + slopeWidth
		water.topFront.transform.localPosition = Vector3.zero
			+ Vector3.left * water.slopeWidth;
		water.topFront.transform.localScale = new Vector3(width - water.slopeWidth, 1f, 1f);

		// ADJUST COLLIDER

		BoxCollider2D boxCollider = water.GetComponent<BoxCollider2D>();
		boxCollider.size = new Vector2(width, height);
		boxCollider.center = new Vector2(-width / 2f, -height / 2f);

	}
}
