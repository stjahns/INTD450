using UnityEngine;
using System.Collections;

public class LevelEnd : MonoBehaviour
{
	public string nextLevel;

	public float fadeOutTime = 1.0f;
	public GameObject fadeEffectPrefab;

	//
	// Lets us pick an editor icon..
	//
	void OnDrawGizmos()
	{
	}

	//
	// When player enters LevelEnd trigger, proceed to next level
	//
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			StartCoroutine(DoLevelEnd());
		}
	}

	private void startFadeOut()
	{
		GameObject fadeEffectObject = Instantiate(fadeEffectPrefab,
				Vector3.zero,
				Quaternion.identity) as GameObject;

		FadeEffect fadeEffect = fadeEffectObject.GetComponent<FadeEffect>();
		fadeEffect.fadeInOnStart = false;
		fadeEffect.fadeTime = fadeOutTime;
		fadeEffect.FadeOut();
	}

	private IEnumerator DoLevelEnd()
	{
		startFadeOut();

		int nextlevel = Application.loadedLevel + 1;
		Save_Load save = new Save_Load ();
		save.score = 200;
		save.level = nextlevel;
		save.player_name = "player";
		save.create_new();

		yield return StartCoroutine(CoroutineExtensions.WaitForRealSeconds(fadeOutTime));
		Application.LoadLevel(nextlevel);
	}
}
