using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZapController : MonoBehaviour
{
	public Transform zapOrigin;
	public List<string> zappableLayers;

	public LineRenderer laserRenderer;
	public AudioClip firingSound;

	public float zapTime = 0.1f;
	public int zapDamage = 1;

	private Transform zapHit;
	private float zapTimer;

	public void Start()
	{
		laserRenderer.enabled = false;
	}

	public void Update()
	{
		zapTimer += Time.deltaTime;

		laserRenderer.SetPosition(0, zapOrigin.position);

		if (zapHit)
		{
			laserRenderer.SetPosition(1, zapHit.position);
		}

		if (zapTimer > zapTime)
		{
			laserRenderer.enabled = false;
		}
	}

	[InputSocket]
	public void Zap()
	{
		// Fire from zapOrigin in its y direction
		int layerMask = 0;
		zappableLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));
		Vector2 direction = zapOrigin.TransformDirection(Vector2.up);
		RaycastHit2D hit = Physics2D.Raycast(zapOrigin.position, direction, layerMask);
		if (hit)
		{
			laserRenderer.enabled = true;

			zapHit = hit.transform;
			laserRenderer.SetPosition(0, zapOrigin.position);
			laserRenderer.SetPosition(1, zapHit.transform.position);

			// Try and damage the thing you zapped
			hit.rigidbody.gameObject.SendMessage("TakeDamage",
					zapDamage,
					SendMessageOptions.DontRequireReceiver);

			zapTimer = 0f;

			if (firingSound)
			{
				AudioSource.PlayClipAtPoint(firingSound, transform.position);
			}
		}
	}
}
