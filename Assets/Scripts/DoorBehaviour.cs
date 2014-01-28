using UnityEngine;
using System.Collections;

public class DoorBehaviour : MonoBehaviour
{
	public Transform closedPosition;
	public Transform openPosition;
	public GameObject doorCollider;

	public void Open()
	{
		Debug.Log("OPENING DOOR");
		doorCollider.transform.position = openPosition.position;
	}

	public void Close()
	{
		Debug.Log("CLOSING DOOR");
		doorCollider.transform.position = closedPosition.position;
	}
}
