using UnityEngine;
using System.Collections;
using SimpleJSON;

public class ChainComponent : MonoBehaviour, SaveableComponent
{
	public Rigidbody2D bodyA;
	public Rigidbody2D bodyB;

	public Vector2 offsetA;
	public Vector2 offsetB;

	public ChainRenderer chainRenderer;

	[HideInInspector]
	public HingeJoint2D jointA;

	[HideInInspector]
	public HingeJoint2D jointB;

	public bool severed = false;

	public AudioClip chopSound;
	[Range(0,1)]
	public float chopVolume;

	public bool fallFromLevel = false;

	public bool saveState = false;

	private Vector3 endA;
	private Vector3 endB;

	private Vector2 severPoint = Vector2.zero;

	public void OnDrawGizmos()
	{
	}

	public bool isDuplicate = false;

	public void SaveState(JSONNode data)
	{
		if (saveState)
		{
			data[gameObject.name]["Severed"].AsBool = severed;
			data[gameObject.name]["SeverPointX"].AsFloat = severPoint.x;
			data[gameObject.name]["SeverPointY"].AsFloat = severPoint.y;
		}
	}

	public void LoadState(JSONNode data)
	{
		if (saveState)
		{
			if (data[gameObject.name] != null && data[gameObject.name]["Severed"].AsBool)
			{
				// sever the chain!
				Vector2 point = new Vector2(data[gameObject.name]["SeverPointX"].AsFloat,
											data[gameObject.name]["SeverPointY"].AsFloat);
				StartCoroutine(DelayedSever(point));
			}
		}
	}

	IEnumerator DelayedSever(Vector2 point)
	{
		yield return 0;
		Sever(point);
	}

	public void Start()
	{
		// Center chain between the connected bodies
		transform.position = (bodyA.transform.position + bodyB.transform.position) / 2.0f;

		if (!isDuplicate)
		{
			// Attach to body A
			jointA = gameObject.AddComponent<HingeJoint2D>();
			jointA.connectedBody = bodyA;
			jointA.anchor = bodyA.transform.position - transform.position;
			jointA.connectedAnchor = offsetA;

			// Attach to body B
			jointB = gameObject.AddComponent<HingeJoint2D>();
			jointB.connectedBody = bodyB;
			jointB.anchor = bodyB.transform.position - transform.position;
			jointB.connectedAnchor = offsetB;
		}

		endA = bodyA.transform.position;
		endB = bodyB.transform.position;

		if (chainRenderer == null)
		{
			chainRenderer = GetComponent<ChainRenderer>();
		}
	}

	void Sever(Vector2 point)
	{
		severPoint = point;

		if (fallFromLevel)
		{
			foreach (var collider in bodyB.GetComponentsInChildren<Collider2D>())
			{
				collider.enabled = false;
			}

			Destroy(bodyB.gameObject, 5);
		}

		// sever connection...
		severed = true;

		// Duplicate this chain...
		GameObject chainDuplicate = Instantiate(gameObject) as GameObject;

		// create 2 new rigid bodies at point of severence

		// Top half
		GameObject severedEndA = new GameObject("SeveredEnd");
		severedEndA.transform.position = point;
		severedEndA.AddComponent<Rigidbody2D>();

		bodyB = severedEndA.rigidbody2D;
		bodyB.mass = 0.01f;
		jointB.connectedBody = bodyB;
		jointB.anchor = bodyB.transform.position - transform.position;

		chainRenderer.end = bodyB.transform;

		// Bottom half... 
		GameObject severedEndB = new GameObject("SeveredEnd");
		severedEndB.transform.position = point;
		severedEndB.AddComponent<Rigidbody2D>();
		severedEndB.rigidbody2D.mass = 0.01f;

		ChainComponent bottomChain = chainDuplicate.GetComponent<ChainComponent>();
		bottomChain.saveState = false;
		bottomChain.isDuplicate = true;
		ChainRenderer bottomRenderer = chainDuplicate.GetComponent<ChainRenderer>();

		bottomChain.bodyA = severedEndB.rigidbody2D;
		bottomChain.jointA.connectedBody = severedEndB.rigidbody2D;
		bottomChain.jointA.anchor = severedEndB.rigidbody2D.transform.position 
			- transform.position;

		bottomRenderer.start = severedEndB.rigidbody2D.transform;

		if (fallFromLevel)
		{
			Destroy(chainDuplicate, 5);
			Destroy(severedEndB, 5);
		}
	}

	public void Update()
	{
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
				AudioSource.PlayClipAtPoint(chopSound, transform.position, chopVolume);
				Sever(hit.point);
			}
		}
	}
}
