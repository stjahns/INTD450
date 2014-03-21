using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class StateMachineBase : MonoBehaviour
{

	public Action DoUpdate = DoNothing;
	public Action DoLateUpdate = DoNothing;
	public Action DoFixedUpdate = DoNothing;

	public Action<Collider2D> DoOnTriggerEnter = DoNothingCollider;
	public Action<Collider2D> DoOnTriggerStay = DoNothingCollider;
	public Action<Collider2D> DoOnTriggerExit = DoNothingCollider;

	public Action<Collision2D> DoOnCollisionEnter = DoNothingCollision;
	public Action<Collision2D> DoOnCollisionStay = DoNothingCollision;
	public Action<Collision2D> DoOnCollisionExit = DoNothingCollision;
	
	public Action DoOnMouseEnter = DoNothing;
	public Action DoOnMouseUp = DoNothing;
	public Action DoOnMouseDown = DoNothing;
	public Action DoOnMouseOver = DoNothing;
	public Action DoOnMouseExit = DoNothing;
	public Action DoOnMouseDrag = DoNothing;

	public Action DoOnGUI = DoNothing;

	public Func<IEnumerator> ExitState = DoNothingCoroutine;

	private Enum _currentState;
	public Enum currentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			_currentState = value;
			ConfigureCurrentState();
		}
	}

	void Update()
	{
		DoUpdate();
	}

	void ConfigureCurrentState()
	{
		if (ExitState != null)
		{
			StartCoroutine(ExitState());
		}

		DoUpdate = ConfigureDelegate<Action>("Update", DoNothing);
		DoOnGUI = ConfigureDelegate<Action>("DoOnGUI", DoNothing);
		DoLateUpdate = ConfigureDelegate<Action>("DoLateUpdate", DoNothing);
		DoFixedUpdate = ConfigureDelegate<Action>("DoFixedUpdate", DoNothing);

		DoOnMouseUp = ConfigureDelegate<Action>("DoOnMouseUp", DoNothing);
		DoOnMouseDown = ConfigureDelegate<Action>("DoOnMouseDown", DoNothing);
		DoOnMouseExit = ConfigureDelegate<Action>("DoOnMouseExit", DoNothing);
		DoOnMouseEnter = ConfigureDelegate<Action>("DoOnMouseEnter", DoNothing);
		DoOnMouseDrag = ConfigureDelegate<Action>("DoOnMouseDrag", DoNothing);

		DoOnTriggerEnter = ConfigureDelegate<Action<Collider2D>>
			("DoOnTriggerEnter", DoNothingCollider);
		DoOnTriggerExit = ConfigureDelegate<Action<Collider2D>>
			("DoOnTriggerExit", DoNothingCollider);
		DoOnTriggerStay = ConfigureDelegate<Action<Collider2D>>
			("DoOnTriggerStay", DoNothingCollider);

		DoOnCollisionEnter = ConfigureDelegate<Action<Collision2D>>
			("DoOnCollisionEnter", DoNothingCollision);
		DoOnCollisionExit = ConfigureDelegate<Action<Collision2D>>
			("DoOnCollisionExit", DoNothingCollision);
		DoOnCollisionStay = ConfigureDelegate<Action<Collision2D>>
			("DoOnCollisionStay", DoNothingCollision);

		Func<IEnumerator> enterState = ConfigureDelegate<Func<IEnumerator>>
			("EnterState", DoNothingCoroutine);
		ExitState = ConfigureDelegate<Func<IEnumerator>>
			("ExitState", DoNothingCoroutine);

		// opt: turn off gui if we don't have an ongui
		// EnableGUI();

		StartCoroutine(enterState());
	}

	// Define a generic method that returns a delegate
	T ConfigureDelegate<T>(string methodRoot, T defaultMethod) where T : class
	{
		// find method called CURRENTSTATE_METHODROOT
		var method = GetType().GetMethod(_currentState.ToString() + "_" + methodRoot,
				BindingFlags.Instance | BindingFlags.Public | 
				BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		if (method != null)
		{
			// Create delegate of the type, and cast
			return Delegate.CreateDelegate(typeof(T), this, method) as T;
		}
		else
		{
			return defaultMethod;
		}
	}
	
	private static void DoNothing()
	{
	}

	private static void DoNothingCollider(Collider2D c)
	{
	}

	private static void DoNothingCollision(Collision2D c)
	{
	}

	private static IEnumerator DoNothingCoroutine()
	{
		yield return 0;
	}
}
