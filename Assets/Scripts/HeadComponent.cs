using UnityEngine;
using System.Collections;

public class HeadComponent : MonoBehaviour {

	public Transform neckConnection;
	public TorsoComponent torso;

	private TorsoComponent connectedTorso;

	//public SpringJoint2D neckJoint;

	public bool connected;

	// Use this for initialization
	void Start () {
		connected = false;
	}
	
	// Update is called once per frame
	void Update () {

		// If a connected component is set
		if (!connected && torso != null)
		{
			torso.Connect(this);
			connectedTorso = torso;
			connected = true;
		}
		else if (connected && torso == null)
		{
			connectedTorso.Disconnect();
			connectedTorso = null;
			connected = false;
		}
	}
}
