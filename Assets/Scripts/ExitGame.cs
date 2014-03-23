using UnityEngine;
using System.Collections;

public class ExitGame : MonoBehaviour
{
	[InputSocket]
	public void Exit()
	{
		Debug.Log("Quitting game...");
		Application.Quit();
	}

	public void OnDrawGizmos()
	{
	}
}
