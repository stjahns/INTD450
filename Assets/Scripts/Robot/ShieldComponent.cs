using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShieldComponent : LimbComponent 
{
	public AudioClip enableClip;
	public AudioClip disableClip;

	public Vector3 disabledScale;
	public float floatForce;

	public GameObject shieldEffectPrefab;
	public Transform shieldEffectOrigin;

	private GameObject shieldEffect;
	private bool shieldActive;

	private Renderer renderer;

	override public void Start ()
	{
		base.Start();

		shieldActive = false;

		shieldEffect = Instantiate(shieldEffectPrefab, shieldEffectOrigin.position, transform.rotation)
			as GameObject;
		shieldEffect.transform.parent = transform;
		renderer = shieldEffect.GetComponent<Renderer>();
		shieldEffect.transform.localScale = disabledScale;
	}

	override public void FireAbility()
	{
		if (shieldActive)
		{
			shieldActive = false;
			SFXSource.PlayOneShot(disableClip);

			// shrink shield
			shieldEffect.transform.localScale = disabledScale;
		}
		else
		{
			shieldActive = true;
			SFXSource.PlayOneShot(enableClip);

			// strech shield
			shieldEffect.transform.localScale = Vector3.one;
		}
	}

	public override void  Update()
	{
		if (shieldEffect)
		{
			shieldEffect.transform.position = shieldEffectOrigin.position;
			shieldEffect.transform.rotation = transform.rotation;
			renderer.sortingLayerID = sprites[0].sortingLayerID;
		}
	}

	override public void FixedUpdate()
	{
		base.FixedUpdate();

		int layerMask = 1 << LayerMask.NameToLayer("Water");
		bool inWater = Physics2D.Linecast(transform.position, groundCheck.position, layerMask);

		if (shieldActive && inWater)
		{
			// apply upward force
			PlayerBehavior.Player.rigidbody2D.AddForce(Vector3.up * floatForce);
		}
	}

	override public void OnRemove()
	{
		base.OnRemove();

		if (shieldActive)
		{
			shieldActive = false;

			SFXSource.PlayOneShot(disableClip);

			// shrink shield
			shieldEffect.transform.localScale = disabledScale;
		}
	}
}
