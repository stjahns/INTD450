using UnityEngine;
using System.Collections;

public class FadeEffect : MonoBehaviour
{
	public bool fadeInOnStart = true;
	public float fadeTime;
	public Texture blackTexture;

	private float alpha;
	private float fadeTimer;
	private float fadeTimerStart;

	public enum State
	{
		FadingIn,
		FadingOut,
		FadedOut,
		FadedIn,
	}

	public State state;

	public void OnDrawGizmos()
	{
	}

	void Start ()
	{
		alpha = 1;
		fadeTimer = 0;
		fadeTimerStart = Time.realtimeSinceStartup;
		AudioListener.volume = 0;
	}
	
	void OnGUI ()
	{

		switch (state)
		{
			case State.FadingIn:
				fadeTimer = Time.realtimeSinceStartup - fadeTimerStart;
				alpha = Mathf.Lerp(1, 0, fadeTimer / fadeTime);
				AudioListener.volume = Mathf.Lerp(0, 1, fadeTimer / fadeTime);
				if (fadeTimer > fadeTime)
				{
					state = State.FadedIn;
				}
				break;
			case State.FadingOut:
				fadeTimer = Time.realtimeSinceStartup - fadeTimerStart;
				alpha = Mathf.Lerp(0, 1, fadeTimer / fadeTime);
				AudioListener.volume = Mathf.Lerp(1, 0, fadeTimer / fadeTime);
				if (fadeTimer > fadeTime)
				{
					state = State.FadedOut;
				}
				break;
		}

		GUI.color = new Color(0, 0, 0, alpha);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
	}


	[InputSocket]
	public void FadeIn()
	{
		fadeTimer = 0;
		alpha = 1;
		state = State.FadingIn;
		AudioListener.volume = 0;
	}

	[InputSocket]
	public void FadeOut()
	{
		fadeTimer = 0;
		alpha = 0;
		state = State.FadingOut;
		AudioListener.volume = 1;
	}
}
