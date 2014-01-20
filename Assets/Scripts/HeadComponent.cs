using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeadComponent : RobotComponent {

	public Transform neckConnection;
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
