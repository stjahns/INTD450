using UnityEngine;
using System.Collections;

public class HeadComponent : MonoBehaviour {

	public Transform neckConnection;
	public Transform groundCheck;
	public TorsoComponent torso;

	private TorsoComponent connectedTorso = null;

	// Use this for initialization
	void Start () {
		CheckNeckConnection();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		CheckNeckConnection();

	}

	public bool checkOnGround() {

		if (connectedTorso) {
			return connectedTorso.checkOnGround();
		}

		int groundOnly = 1 << LayerMask.NameToLayer("Ground");
		return Physics2D.Linecast(transform.position, groundCheck.position, groundOnly);
	}

	void CheckNeckConnection() {

		// If a connected component is set
		if (!connectedTorso && torso)
		{
			Debug.Log("Torso Connected");
			torso.Connect(this);
			connectedTorso = torso;
			Animator anim = GetComponentInChildren<Animator>();
			anim.animatePhysics = false;
			anim.animatePhysics = true;
		}
		else if (connectedTorso && !torso)
		{
			connectedTorso.Disconnect();
			connectedTorso = null;
		}
	}
}
