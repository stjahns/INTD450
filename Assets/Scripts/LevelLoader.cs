using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
	public float fadeOutTime = 1.0f;

	public string menuLevel;
	public GameObject fadeEffectPrefab;

	private int levelIndex;

	public void OnDrawGizmos()
	{
	}

	[InputSocket]
	public void ResetLevel()
	{
		LoadLevel(Application.loadedLevel);
	}

	[InputSocket]
	public void LoadMenu()
	{
		startFadeOut();
		StartCoroutine(DoLoadMenu());
	}

	private IEnumerator DoLoadMenu()
	{
		yield return StartCoroutine(CoroutineExtensions.WaitForRealSeconds(fadeOutTime));
		Time.timeScale = 1;
		Application.LoadLevel(menuLevel);
	}

	[InputSocket]
	public void ResumeLevel()
	{
		Save_Load load = new Save_Load();
		load.player_name="player";
		var data=load.file_load();
		int level=System.Convert.ToInt32(data["array"][1]["Level"]);
		if (level==0)
		{
			level=1;
		}

		LoadLevel(level);
	}

	[InputSocket]
	public void StartGame()
	{
		// New save
		Save_Load load = new Save_Load();
		load.player_name="player";
		load.create_new();

		LoadLevel(1);
	}

	[InputSocket]
	public void LoadLevel(string index)
	{
		LoadLevel(int.Parse(index));
	}

	public void LoadLevel(int index)
	{
		startFadeOut();
		levelIndex = index;
		StartCoroutine(DoLoad());
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

	private IEnumerator DoLoad()
	{
		Debug.Log(string.Format("Loading Level {0}..", levelIndex));
		yield return StartCoroutine(CoroutineExtensions.WaitForRealSeconds(fadeOutTime));
		Application.LoadLevel(levelIndex);
	}

}
