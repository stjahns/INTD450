using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovementController : MonoBehaviour {

	public PlayerBehavior player;
	public PlayerAttachmentController attachmentController;

	public float maxGroundSpeed = 3.0f;
	public float groundMoveForce = 1.0f;
	
	public float airMoveForce = 1.0f;
	
	public AudioClip headJumpClip;
	public AudioClip jumpClip;
	public float jumpForce = 1.0f;
	public float jumpCooloff = 0.25f;

	public AudioClip switchAbilityClip;

	[HideInInspector]
	public bool MouseAim =  true;

	private float jumpTimer = 0.0f;
	
	// Update is called once per frame
	void Update ()
	{
		Animator anim = GetComponentInChildren<Animator>();

		if (Input.GetKeyDown(KeyCode.F) && !attachmentController.enabled)
		{
			// Enable  attachment mode
			attachmentController.enabled = true;
		}

		// Swtich arm abilities
		if (Input.GetKeyDown(KeyCode.Q))
		{
			AudioSource3D.PlayClipAtPoint(switchAbilityClip, transform.position);
			player.NextArmAbility();
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			AudioSource3D.PlayClipAtPoint(switchAbilityClip, transform.position);
			player.NextLegAbility();
		}

		if (jumpTimer > 0.0f)
		{
			jumpTimer -= Time.deltaTime;
		}

		if (player.OnGround && jumpTimer <= 0.0f && Input.GetKeyDown(KeyCode.Space)) {
			// jump
			jumpTimer = jumpCooloff;

			if (player.HasLimbs)
			{
				AudioSource3D.PlayClipAtPoint(jumpClip, transform.position);
			}
			else
			{
				AudioSource3D.PlayClipAtPoint(headJumpClip, transform.position);
			}

			rigidbody2D.AddForce(Vector2.up * jumpForce);
			anim.SetTrigger("jump");
		}

		if (Input.GetMouseButtonDown(0))
		{
			player.FireArmAbility();
		}

		if (Input.GetMouseButtonDown(1))
		{
			player.FireLegAbility();
		}

		if (MouseAim)
		{
			Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			if ((mousePosition - transform.position).x > 0)
			{
				// face right
				anim.SetBool("facingLeft", false);
				player.skeleton.direction = PlayerSkeleton.Direction.Right;
			}
			else
			{
				// face left
				anim.SetBool("facingLeft", true);
				player.skeleton.direction = PlayerSkeleton.Direction.Left;
			}

			if (player.GetActiveArm() && player.GetActiveArm().shouldAim)
			{
				Vector2 jointOrigin = player.GetActiveArm().parentAttachmentPoint.transform.position;
				Vector2 aimOrigin = Camera.main.WorldToScreenPoint(jointOrigin);
				Vector2 playerToPointer;

				playerToPointer.x = Input.mousePosition.x - aimOrigin.x;
				playerToPointer.y = Input.mousePosition.y - aimOrigin.y;
				playerToPointer.Normalize();

				string xVar = player.GetActiveArm().parentAttachmentPoint.aimX;
				string yVar = player.GetActiveArm().parentAttachmentPoint.aimY;

				if (!player.facingLeft)
				{
					playerToPointer.x *= -1;
				}

				anim.SetFloat(xVar, playerToPointer.x);
				anim.SetFloat(yVar, playerToPointer.y);
			}
		}
	}

	void FixedUpdate ()
	{
		if (Input.GetKey(KeyCode.A) && rigidbody2D.velocity.x > -maxGroundSpeed)
		{
			// move left
			if (player.OnGround)
			{
				rigidbody2D.AddForce(Vector2.right * -groundMoveForce);
			}
			else
			{
				rigidbody2D.AddForce(Vector2.right * -airMoveForce);
			}
		}

		if (Input.GetKey(KeyCode.D) && rigidbody2D.velocity.x < maxGroundSpeed)
		{
			// move right
			if (player.OnGround)
			{
				rigidbody2D.AddForce(Vector2.right * groundMoveForce);
			}
			else
			{
				rigidbody2D.AddForce(Vector2.right * airMoveForce);
			}
		}
	}
}
