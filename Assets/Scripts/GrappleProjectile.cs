﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrappleProjectile : MonoBehaviour
{

	public float pointAttraction;
	public float projectileDrag = 100f;
	public SpriteRenderer clampSprite;

	public Sprite collapsedSprite;
	public Sprite openSprite;
	public Sprite clampedSprite;

	public bool AttachedToPoint { get; set; }

	private Transform parent;
	private Rigidbody2D body;

	private int layerMask;

	public delegate void GrappleHitHandler(Collision2D hit);
	public event GrappleHitHandler GrappleHit;

	// Use this for initialization
	void Start ()
	{
		AttachedToPoint = false;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if (body && body.velocity.magnitude > 1)
		{
			transform.rotation = Quaternion.FromToRotation(Vector3.down, body.velocity);
		}
	}

	public void FireProjectile(Vector3 velocity, int mask)
	{
		layerMask = mask;
		parent = transform.parent;
		transform.parent = null;
		transform.localScale = Vector3.one;
		transform.rotation = Quaternion.FromToRotation(Vector3.down, velocity);

		if (body == null)
		{
			body = gameObject.AddComponent<Rigidbody2D>();
			body.velocity = velocity;
		}

		clampSprite.sprite = openSprite;
	}

	public void ResetProjectile()
	{
		if (body)
		{
			body.isKinematic = true;
			Destroy(body);
			body = null;
		}

		if (parent)
		{
			transform.parent = parent;
		}

		transform.localEulerAngles = Vector3.zero;
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;

		AttachedToPoint = false;

		clampSprite.sprite = collapsedSprite;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (AttachedToPoint)
			return;

		if (body && (1 << collision.collider.gameObject.layer & layerMask) != 0)
		{
			// Hit something!
			body.isKinematic = true;

			AttachedToPoint = true;

			clampSprite.sprite = clampedSprite;
			transform.rotation = Quaternion.FromToRotation(Vector3.down, 
					collision.transform.position.XY() - transform.position.XY());

			if (GrappleHit != null)
			{
				GrappleHit(collision);
			}
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (AttachedToPoint)
			return;

		if (body && (1 << other.gameObject.layer & layerMask) != 0)
		{
			// Near a grapple point, suck to it!
			body.AddForce((other.transform.position - transform.position) * pointAttraction);
			body.drag = projectileDrag;
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (AttachedToPoint)
			return;

		if (body && (1 << other.gameObject.layer & layerMask) != 0)
		{
			body.drag = 0.0f;;
		}
	}
}