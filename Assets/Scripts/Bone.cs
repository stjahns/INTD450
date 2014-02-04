using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bone : MonoBehaviour
{
	public Bone LowerJoint;

	public List<Bone> childBones;

	public int spriteOrder = 0;
	public bool spriteMirrored = false;

	public void OnDrawGizmos()
	{
		Gizmos.DrawSphere(transform.position, 0.04f); 
		foreach (Bone child in childBones)
		{
			Gizmos.DrawLine(transform.position, child.transform.position);
		}
	}
}
