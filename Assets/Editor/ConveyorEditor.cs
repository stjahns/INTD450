using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

[CustomEditor(typeof(ConveyorBehavior), true)]
public class ConveyorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Generate Conveyor"))
		{
			Generate();
		}


		base.OnInspectorGUI();
	}

	void Generate()
	{

		ConveyorBehavior conveyorBehavior = target as ConveyorBehavior;
		GameObject conveyorObject = conveyorBehavior.gameObject;

		if (conveyorBehavior.beltContainer != null)
		{
			Undo.DestroyObjectImmediate(conveyorBehavior.beltContainer);
		}

		GameObject beltContainer = new GameObject("Belt");
		beltContainer.transform.parent = conveyorObject.transform;
		beltContainer.transform.position = Vector3.zero;


		var property = serializedObject.FindProperty("beltContainer");
		property.objectReferenceValue = beltContainer;

		if (conveyorBehavior.axleContainer != null)
		{
			Undo.DestroyObjectImmediate(conveyorBehavior.axleContainer);
		}

		GameObject axleContainer = new GameObject("Axles");
		axleContainer.transform.parent = conveyorObject.transform;
		axleContainer.transform.position = Vector3.zero;
		property = serializedObject.FindProperty("axleContainer");
		property.objectReferenceValue = axleContainer;

		if (conveyorBehavior.guideContainer != null)
		{
			Undo.DestroyObjectImmediate(conveyorBehavior.guideContainer);
		}

		GameObject guideContainer = new GameObject("Guides");
		guideContainer.transform.parent = conveyorObject.transform;
		guideContainer.transform.position = Vector3.zero;
		property = serializedObject.FindProperty("guideContainer");
		property.objectReferenceValue = guideContainer;

		serializedObject.ApplyModifiedProperties();


		conveyorBehavior.conveyorMotors.Clear();

		GenerateAxles();
		GenerateBelt();
		GenerateGuides();
	}

	void GenerateAxles()
	{
		ConveyorBehavior conveyorBehavior = target as ConveyorBehavior;

		float length = Vector3.Distance(conveyorBehavior.start.position,
				conveyorBehavior.end.position);

		// Create axles...
		int numAxles = Mathf.FloorToInt(length * conveyorBehavior.axlesPerUnit);

		GameObject axle;

		axle = Instantiate(conveyorBehavior.axlePrefab, conveyorBehavior.start.position, Quaternion.identity) as GameObject;
		axle.transform.parent = conveyorBehavior.axleContainer.transform;

		// add axle motor to list
		HingeJoint2D axleMotor = axle.GetComponentInChildren<HingeJoint2D>();
		if (axleMotor)
		{
			conveyorBehavior.conveyorMotors.Add(axleMotor);
		}

		axle = Instantiate(conveyorBehavior.axlePrefab, conveyorBehavior.end.position, Quaternion.identity) as GameObject;
		axle.transform.parent = conveyorBehavior.axleContainer.transform;

		axleMotor = axle.GetComponentInChildren<HingeJoint2D>();
		if (axleMotor)
		{
			conveyorBehavior.conveyorMotors.Add(axleMotor);
		}

		for (int i = 1; i < numAxles - 1; ++i)
		{
			Vector3 position = Vector3.Lerp(conveyorBehavior.start.position,
					conveyorBehavior.end.position,
					(float) i / (float) (numAxles - 1));

			axle = Instantiate(conveyorBehavior.axlePrefab, position, Quaternion.identity) as GameObject;
			axle.transform.parent = conveyorBehavior.axleContainer.transform;

			// add axle motor to list
			axleMotor = axle.GetComponentInChildren<HingeJoint2D>();
			if (axleMotor)
			{
				conveyorBehavior.conveyorMotors.Add(axleMotor);
			}
		}
	}

	void GenerateBelt()
	{
		ConveyorBehavior conveyorBehavior = target as ConveyorBehavior;

		Rigidbody2D startLink = null;
		Rigidbody2D endLink = null;

		Vector3 start = conveyorBehavior.start.position;
		Vector3 end = conveyorBehavior.end.position;

		Vector3 startToEnd = end - start;
		Vector3 up = Quaternion.FromToRotation(Vector3.right, Vector3.up) * startToEnd;
		up.Normalize();

		float axleRadius = conveyorBehavior.axleRadius;

		GenerateStraightBelt(
				start + up * axleRadius,
				end + up * axleRadius,
				ref startLink,
				ref endLink);

		GenerateBeltEnd(
				end,
				startToEnd.normalized,
				ref startLink,
				ref endLink);

		GenerateStraightBelt(
				end + up * -axleRadius,
				start + up * -axleRadius,
				ref startLink,
				ref endLink);

		GenerateBeltEnd(
				start,
				-startToEnd.normalized,
				ref startLink,
				ref endLink);

		// Close the loop
		DistanceJoint2D joint = endLink.GetComponent<DistanceJoint2D>();
		joint.connectedBody = startLink;
		joint.distance = conveyorBehavior.linkDistance;
	}

	void GenerateStraightBelt(
			Vector3 start,
			Vector3 end,
			ref Rigidbody2D startLink,
			ref Rigidbody2D endLink)
	{
		ConveyorBehavior conveyorBehavior = target as ConveyorBehavior;

		Vector3 startToEnd = end - start;
		Vector3 up = Quaternion.FromToRotation(Vector3.right, Vector3.up) * startToEnd;

		float length = Vector3.Distance(start, end);
		int numLinks = Mathf.FloorToInt(length * conveyorBehavior.linksPerUnit);

		GameObject link;
		DistanceJoint2D joint;

		link = Instantiate(conveyorBehavior.beltLinkPrefab, start, 
				Quaternion.FromToRotation(Vector3.up, up)) as GameObject;
		link.transform.eulerAngles = new Vector3(0, 0, link.transform.eulerAngles.z);
		link.transform.parent = conveyorBehavior.beltContainer.transform;

		if (startLink == null && endLink == null)
		{
			startLink = link.rigidbody2D;
			endLink = link.rigidbody2D;
		}
		else
		{
			joint = endLink.GetComponent<DistanceJoint2D>();
			joint.connectedBody = link.rigidbody2D;
			joint.distance = conveyorBehavior.linkDistance;
			endLink = link.rigidbody2D;
		}

		for (int i = 1; i < numLinks - 1; ++i)
		{
			Vector3 position = Vector3.Lerp(start, end, (float) i / (float) (numLinks - 1));
			link = Instantiate(conveyorBehavior.beltLinkPrefab, position, 
					Quaternion.FromToRotation(Vector3.up, up)) as GameObject;
			link.transform.eulerAngles = new Vector3(0, 0, link.transform.eulerAngles.z);
			link.transform.parent = conveyorBehavior.beltContainer.transform;

			joint = endLink.GetComponent<DistanceJoint2D>();
			joint.connectedBody = link.rigidbody2D;
			joint.distance = conveyorBehavior.linkDistance;
			endLink = link.rigidbody2D;
		}

		link = Instantiate(conveyorBehavior.beltLinkPrefab, end, 
				Quaternion.FromToRotation(Vector3.up, up)) as GameObject;
		link.transform.eulerAngles = new Vector3(0, 0, link.transform.eulerAngles.z);
		link.transform.parent = conveyorBehavior.beltContainer.transform;

		joint = endLink.GetComponent<DistanceJoint2D>();
		joint.connectedBody = link.rigidbody2D;
		joint.distance = conveyorBehavior.linkDistance;
		endLink = link.rigidbody2D;
	}

	void GenerateBeltEnd(
			Vector3 center,
			Vector3 direction,
			ref Rigidbody2D startLink,
			ref Rigidbody2D endLink)
	{
		ConveyorBehavior conveyorBehavior = target as ConveyorBehavior;

		DistanceJoint2D joint;

		float axleRadius = conveyorBehavior.axleRadius;

		float arcLength = axleRadius * Mathf.PI;
		int numLinks = Mathf.CeilToInt(arcLength * conveyorBehavior.linksPerUnit);

		Vector3 up = Quaternion.FromToRotation(Vector3.right, Vector3.up) * direction;
		up.Normalize();

		for (int i = 1; i < numLinks - 1; ++i)
		{
			// build CCW...
			float arcFraction = -Mathf.Lerp(0, Mathf.PI, (float) i / (float) (numLinks - 1));

			Quaternion rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * arcFraction, Vector3.forward);
			Vector3 position = center + rotation * (up * axleRadius);
			GameObject link = Instantiate(conveyorBehavior.beltLinkPrefab, position, rotation) as GameObject;
			link.transform.parent = conveyorBehavior.beltContainer.transform;

			joint = endLink.GetComponent<DistanceJoint2D>();
			joint.connectedBody = link.rigidbody2D;
			joint.distance = conveyorBehavior.linkDistance;
			endLink = link.rigidbody2D;
		}
	}

	void GenerateGuides()
	{
		ConveyorBehavior conveyorBehavior = target as ConveyorBehavior;


		Vector3 start = conveyorBehavior.start.position;
		Vector3 end = conveyorBehavior.end.position;

		Vector3 startToEnd = end - start;

		Vector3 up = Quaternion.FromToRotation(Vector3.right, Vector3.up) * startToEnd;
		up.Normalize();

		float length = Vector3.Distance(start, end);

		GameObject guide;
		BoxCollider2D box;
		CircleCollider2D circle;

		// Inner guard
		guide = new GameObject("guide");
		guide.transform.position = 0.5f * (start + end);
		guide.transform.Rotate(Quaternion.FromToRotation(Vector3.up, up).eulerAngles);

		box = guide.AddComponent<BoxCollider2D>();
		box.size = new Vector2(length, conveyorBehavior.axleRadius);
		box.center = Vector2.zero;
		guide.transform.parent = conveyorBehavior.guideContainer.transform;
		guide.layer = LayerMask.NameToLayer(conveyorBehavior.guideLayer);

		// Bottom guard
		guide = new GameObject("guide");
		guide.transform.position = 0.5f * (start + end);
		guide.transform.Rotate(Quaternion.FromToRotation(Vector3.up, up).eulerAngles);

		// FIXME - something's wrong for vertical conveyors

		box = guide.AddComponent<BoxCollider2D>();
		box.size = new Vector2(length, conveyorBehavior.axleRadius);
		box.center = - up * conveyorBehavior.axleRadius * 2f;

		circle = guide.AddComponent<CircleCollider2D>();
		circle.center = Vector3.right * length * -0.5f - conveyorBehavior.axleRadius * 2f * up;
		circle.radius = conveyorBehavior.axleRadius / 2f;

		circle = guide.AddComponent<CircleCollider2D>();
		circle.center = Vector3.right * length * 0.5f - conveyorBehavior.axleRadius * 2f * up;
		circle.radius = conveyorBehavior.axleRadius / 2f;

		guide.transform.parent = conveyorBehavior.guideContainer.transform;
		guide.layer = LayerMask.NameToLayer(conveyorBehavior.guideLayer);

	}
}

