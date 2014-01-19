using UnityEngine;
using System.Collections;

public class PlayerBehavior : MonoBehaviour {

	public float moveForce = 1.0f;
	public float jumpForce = 1.0f;

	private bool jumping = false;

	private Animator anim = null;

	public Transform leftArmJoint;

	// Use this for initialization
	void Start () {
		jumping = false;
		anim = GetComponentInChildren<Animator>();
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

		if (!jumping && Input.GetKey(KeyCode.Space)) {
			// jump
			if (leftArmJoint.position.y < 3.0)
			{
			rigidbody2D.AddForce(Vector2.up * jumpForce);
			}
			//jumping = true;
		}

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
