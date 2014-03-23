using UnityEngine;
using System.Collections;

public class ExitGame : MonoBehaviour
{
	public float fadeOutTime = 1.0f;

	public GameObject fadeEffectPrefab;

	[InputSocket]
	public void Exit()
	{
		GameObject fadeEffectObject = Instantiate(fadeEffectPrefab,
				Vector3.zero,
				Quaternion.identity) as GameObject;

		FadeEffect fadeEffect = fadeEffectObject.GetComponent<FadeEffect>();
		fadeEffect.fadeInOnStart = false;
		fadeEffect.fadeTime = fadeOutTime;
		fadeEffect.FadeOut();

		StartCoroutine(DoExit());
	}

	private IEnumerator DoExit()
	{
		Debug.Log("Quitting game...");
		yield return StartCoroutine(CoroutineExtensions.WaitForRealSeconds(fadeOutTime));
		Application.Quit();
	}

	public void OnDrawGizmos()
	{
	}
}
