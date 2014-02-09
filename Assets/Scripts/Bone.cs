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

	void Start()
	{
		GameObject forwardObject = new GameObject("Forward");
		forwardObject.transform.parent = transform;
		forwardObject.transform.localPosition = new Vector3(0, 1, 0);
		forward = forwardObject.transform;
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
}
