using UnityEngine;
using System.Collections;

public class PlayerAttachmentController : MonoBehaviour {

	public PlayerBehavior player;
	public PlayerMovementController movementController;

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			// Switch to regular mode
			player.SetController(movementController);
		}
	}
}
