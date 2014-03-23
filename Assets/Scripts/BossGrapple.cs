using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BossGrapple : GrappleComponent
{
	public event Action<GameObject> GrabbedObstacle;

	override protected void OnGrappleHit(Collision2D hit)
	{
		if (hit.gameObject.tag == "BossGrappleTarget")
		{
			//Detach thing from it's parent!?
			DistanceJoint2D joint = hit.gameObject.GetComponent<DistanceJoint2D>();
			hit.gameObject.tag = "Untagged";
			hit.gameObject.transform.parent = null;
			Destroy(joint, 0.5f);

			if (GrabbedObstacle != null)
			{
				GrabbedObstacle(hit.gameObject);
			}
		}

	}
}
