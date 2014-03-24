using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CannonComponent : LimbComponent 
{
	public Animator animator;

	public Transform shotOrigin;

	public int chargeCount = 2;
	public float chargeTime = 2.0f;

	public AudioClip fireClip;
	public AudioClip emptyClip;
	public AudioClip rechargeClip;

	public List<Renderer> chargeRenderers;

	public List<string> beamCollisionLayers;
	public float beamTime = 0.1f;
	public LineRenderer beamRenderer;
	public GameObject hitEffectPrefab;

	public Renderer boostEffect;
	public float boostForce;
	public float boostEffectTime;

	private float boostEffectTimer;

	protected Transform forward;

	protected float chargeTimer;
	protected int charges;

	protected float beamTimer;

	override public void Start ()
	{
		base.Start();

		GameObject forwardObject = new GameObject("Forward");
		forwardObject.transform.parent = transform;
		forwardObject.transform.localPosition = new Vector3(0, -1, 0);
		forward = forwardObject.transform;

		charges = chargeCount;

		ResetChargeRenderers();
	}

	override public void Update()
	{
		base.Update();

		if (charges < chargeCount)
		{
			chargeTimer += Time.deltaTime;
			if (chargeTimer > chargeTime)
			{
				++charges;
				chargeTimer = 0f;
				ResetChargeRenderers();
				SFXSource.PlayOneShot(rechargeClip);
			}
		}

		if (beamTimer > 0)
		{
			beamTimer -= Time.deltaTime;

			// don't think this is necessary
			Vector3 direction = forward.position - transform.position;
			direction.Normalize();

			int layerMask = 0;
			beamCollisionLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));
			RaycastHit2D hit = Physics2D.Raycast(shotOrigin.position, direction, Mathf.Infinity, layerMask);
			if (hit)
			{
				beamRenderer.SetPosition(0, shotOrigin.position);
				beamRenderer.SetPosition(1, hit.point);
			}

			if (beamTimer < 0)
			{
				beamRenderer.enabled = false;
			}
		}

		if (boostEffectTimer > 0)
		{
			boostEffectTimer -= Time.deltaTime;

			if (boostEffectTimer < 0)
			{
				boostEffect.enabled = false;
			}
		}
	}

	public void FixedUpdate()
	{
		if (boostEffectTimer > 0)
		{
			// apply force
			PlayerBehavior.Player.rigidbody2D.AddForce(Vector3.up * boostForce);
		}
	}

	override public void FireAbility()
	{
		if (charges > 0)
		{
			if (IsArm)
			{
				// fire beamy explosiony thing

				animator.SetTrigger("Fire");
				SFXSource.PlayOneShot(fireClip);

				// don't think this is necessary
				Vector3 direction = forward.position - transform.position;
				direction.Normalize();

				// fire raycast in limbs direction
				int layerMask = 0;
				beamCollisionLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));
				RaycastHit2D hit = Physics2D.Raycast(shotOrigin.position, direction, Mathf.Infinity,
						layerMask);
				if (hit)
				{
					beamRenderer.enabled = true;
					beamRenderer.SetPosition(0, shotOrigin.position);
					beamRenderer.SetPosition(1, hit.point);
					beamTimer = beamTime;

					// spawn hit effect
					Instantiate(hitEffectPrefab, hit.point , Quaternion.identity);
				}
				charges -= 1;
				chargeTimer = 0f;
			}
			else
			{
				// jump jet boost
				animator.SetTrigger("Fire");
				SFXSource.PlayOneShot(fireClip);


				boostEffect.enabled = true;
				boostEffectTimer = boostEffectTime;

				charges -= 1;
				chargeTimer = 0f;
			}

			ResetChargeRenderers();
		}
		else
		{
			SFXSource.PlayOneShot(emptyClip);
		}
	}

	void ResetChargeRenderers()
	{
		chargeRenderers.ForEach(r => r.enabled = false);

		for (int i = 0; i < charges; ++i)
		{
			chargeRenderers[i].enabled = true;
		}
	}
}
