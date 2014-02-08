using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovementController : MonoBehaviour {

	public PlayerBehavior player;
	public PlayerAttachmentController attachmentController;

	public float moveForce = 1.0f;
	
	public AudioClip jumpClip;
	public float jumpForce = 1.0f;
	public float jumpCooloff = 0.25f;

	private bool onGround = false;
	private float jumpTimer = 0.0f;
	
	// Update is called once per frame
	void Update ()
	{
		Animator anim = GetComponentInChildren<Animator>();

		if (Input.GetKeyDown(KeyCode.F))
		{
			// Switch to attachment mode
			player.SetController(attachmentController);
		}

		// Swtich arm abilities
		if (Input.GetKeyDown(KeyCode.Q))
		{
			player.NextArmAbility();
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			player.NextLegAbility();
		}

		if (jumpTimer > 0.0f)
		{
			jumpTimer -= Time.deltaTime;
		}

		if (player.OnGround && jumpTimer <= 0.0f && Input.GetKeyDown(KeyCode.Space)) {
			// jump
			jumpTimer = jumpCooloff;
			AudioSource.PlayClipAtPoint(jumpClip, transform.position);
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
	}

	void FixedUpdate ()
	{
		if (Input.GetKey(KeyCode.A))
		{
			// move left
			rigidbody2D.AddForce(Vector2.right * -moveForce);
		}

		if (Input.GetKey(KeyCode.D))
		{
			// move right
			rigidbody2D.AddForce(Vector2.right * moveForce);
		}
	}
}
