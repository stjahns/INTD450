using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bone : MonoBehaviour
{
	public Bone LowerJoint;

	public List<Bone> childBones;

	public float spriteOrder = 0;
	public bool spriteMirrored = false;

	private Transform forward;

	private List<Transform> childTransforms;

	void Awake()
	{
		GameObject forwardObject = new GameObject("Forward");
		forwardObject.transform.parent = transform;
		forwardObject.transform.localPosition = new Vector3(0, 1, 0);
		forward = forwardObject.transform;

		childTransforms = new List<Transform>();
	}

	void LateUpdate()
	{
		foreach (var childTransform in childTransforms)
		{
			childTransform.position = transform.position;
			childTransform.rotation = GetBoneRotation();
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawSphere(transform.position, 0.04f); 
		foreach (Bone child in childBones)
		{
			Gizmos.DrawLine(transform.position, child.transform.position);
		}
	}

	public Quaternion GetBoneRotation()
	{
		Vector3 direction = forward.position - transform.position;
		direction.Normalize();
		return Quaternion.FromToRotation(Vector3.up, direction);
	}

	public void Attach(Transform childTransform)
	{
		childTransforms.Add(childTransform);

	}

	public void Detach(Transform childTransform)
	{
		childTransforms.Remove(childTransform);
	}
}
