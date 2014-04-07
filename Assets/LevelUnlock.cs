using UnityEngine;
using System.Collections;
using SimpleJSON;

public class LevelUnlock : StateMachineBase
{
	public LevelLoader loader;

	public enum State
	{
		P1, O1, O2, P2
	}

	public void Start()
	{
		currentState = State.P1;
	}

	void P1_Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			currentState = State.O1;
		}
	}

	void O1_Update()
	{
		if (Input.GetKeyDown(KeyCode.O))
		{
			currentState = State.O2;
		}
	}

	void O2_Update()
	{
		if (Input.GetKeyDown(KeyCode.O))
		{
			currentState = State.P2;
		}
	}

	void P2_Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			UnlockLevels();
			currentState = State.P1;
		}
	}

	void UnlockLevels()
	{
		int level = Application.loadedLevel;
		Save_Load save = new Save_Load ();
		save.player_name="player";
		save.add_checkpoint(12, "", "", Vector3.zero);
		loader.LoadLevel(0);
	}

	public void OnDrawGizmos()
	{
	}
}
