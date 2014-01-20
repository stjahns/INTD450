using UnityEngine;
using System.Collections;

public class PlayerBehavior : MonoBehaviour {

	public float moveForce = 1.0f;
	public float jumpForce = 1.0f;

	public bool onGround = false;

	private Animator anim = null;

	public HeadComponent head;

	// Use this for initialization
	void Start () {
		anim = GetComponentInChildren<Animator>();
	}

	void Update () {
		onGround = head.checkOnGround();
		//int groundOnly = 1 << LayerMask.NameToLayer("Ground");
		//onGround = Physics2D.Linecast(transform.position, groundCheck.position, groundOnly);

	}

	// Update is called once per frame
	void FixedUpdate () {

		if (Input.GetKey(KeyCode.A)) {
			// move left
			rigidbody2D.AddForce(Vector2.right * -moveForce);
		}

		if (Input.GetKey(KeyCode.D)) {
			// move right
			rigidbody2D.AddForce(Vector2.right * moveForce);
		}

		if (onGround && Input.GetKeyDown(KeyCode.Space)) {
			// jump
			rigidbody2D.AddForce(Vector2.up * jumpForce);
		}

		anim = GetComponentInChildren<Animator>();
		if (anim) {
			anim.SetFloat("lateralVelocity", Mathf.Abs(rigidbody2D.velocity.x));

			Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
			Vector2 playerToPointer;

			playerToPointer.x = Input.mousePosition.x - playerScreenPos.x;
	    	playerToPointer.y = Input.mousePosition.y - playerScreenPos.y;
			playerToPointer.Normalize();
			anim.SetFloat("mouseX", playerToPointer.x);
			anim.SetFloat("mouseY", playerToPointer.y);
		}

	}
}
