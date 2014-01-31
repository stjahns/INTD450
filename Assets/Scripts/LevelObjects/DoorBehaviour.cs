using UnityEngine;
using System.Collections;

public class DoorBehaviour : MonoBehaviour
{
	public Transform closedPosition;
	public Transform openPosition;
	public GameObject doorCollider;

	public bool debug = false;

	public AudioClip openClip;
	public AudioClip closeClip;

	[InputSocket]
	public void Open()
	{
		if (debug)
		{
			Debug.Log("OPENING DOOR");
		}
		doorCollider.transform.position = openPosition.position;

		AudioSource.PlayClipAtPoint(openClip, transform.position);
	}

	[InputSocket]
	public void Close()
	{
		if (debug)
		{
			Debug.Log("CLOSING DOOR");
		}
		doorCollider.transform.position = closedPosition.position;

		AudioSource.PlayClipAtPoint(closeClip, transform.position);
	}
}
