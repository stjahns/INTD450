using UnityEngine;
using System.Collections;

public class ChainComponent : MonoBehaviour
{
	public Rigidbody2D bodyA;
	public Rigidbody2D bodyB;

	public float chainWidth;
	public Material chainMaterial;

	public bool liveEdit = false;

	private LineRenderer chainLine;
	private HingeJoint2D jointA;
	private HingeJoint2D jointB;

	private bool severed = false;

	private Vector3 endA;
	private Vector3 endB;

	public void OnDrawGizmos()
	{
	}

	public void Start()
	{
		// Center chain between the connected bodies
		transform.position = (bodyA.transform.position + bodyB.transform.position) / 2.0f;
		
		// Attach to body A
		jointA = gameObject.AddComponent<HingeJoint2D>();
		jointA.connectedBody = bodyA;
		jointA.anchor = bodyA.transform.position - transform.position;

		// Attach to body B
		jointB = gameObject.AddComponent<HingeJoint2D>();
		jointB.connectedBody = bodyB;
		jointB.anchor = bodyB.transform.position - transform.position;

		endA = bodyA.transform.position;
		endB = bodyB.transform.position;

		// Create line renderer
		chainLine = gameObject.AddComponent<LineRenderer>();
		chainLine.material = chainMaterial;
		chainLine.SetWidth(chainWidth, chainWidth);
		chainLine.SetPosition(0, endA);
		chainLine.SetPosition(1, endB);

	}

	public void Update()
	{
		if (liveEdit)
		{
			chainLine.SetWidth(chainWidth, chainWidth);
			chainLine.material = chainMaterial;
		}

		if (!severed)
		{
			endA = bodyA.transform.position;
			endB = bodyB.transform.position;

			// Do a linecast to see if we are intersected by an active sawblade
			// collider

			int layerMask = 1 << LayerMask.NameToLayer("Sawblade");
			RaycastHit2D hit = Physics2D.Linecast(bodyA.transform.position,
					bodyB.transform.position,
					layerMask);
			if (hit)
			{
				// sever connection
				Destroy(jointA);
				Destroy(jointB);
				severed = true;
			}
		}
		else
		{
			// Chain falls with rigid body
			Vector3 aToB = endB - endA;
			endB = rigidbody2D.transform.position + (aToB / 2.0f);
			endA = rigidbody2D.transform.position - (aToB / 2.0f);
		}

		chainLine.SetPosition(0, endA);
		chainLine.SetPosition(1, endB);
	}
}
