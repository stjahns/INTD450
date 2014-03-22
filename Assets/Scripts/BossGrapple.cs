using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossGrapple : GrappleComponent
{
	override protected void OnGrappleHit(Collision2D hit)
	{
		if (hit.gameObject.tag == "BossGrappleTarget")
		{
			//Detach thing from it's parent!?
			DistanceJoint2D joint = hit.gameObject.GetComponent<DistanceJoint2D>();
			hit.gameObject.tag = null;
			hit.gameObject.transform.parent = null;
			Destroy(joint, 0.5f);
		}

	}
}
