using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class AnimatorWrapper : MonoBehaviour
{
	private Animator _animator;

	void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	// TODO - socket messages need to support more than one argument

	[InputSocket]
	public void AnimatorSetBool(string name, string boolValue)
	{
		_animator.SetBool(name, bool.Parse(boolValue));
	}

	[InputSocket]
	public void AnimatorSetFloat(string name, string floatValue)
	{
		_animator.SetFloat(name, float.Parse(floatValue));
	}

	[InputSocket]
	public void AnimatorSetInt(string name, string intValue)
	{
		_animator.SetFloat(name, int.Parse(intValue));
	}

	[InputSocket]
	public void AnimatorSetTrigger(string name)
	{
		_animator.SetTrigger(name);
	}
}
