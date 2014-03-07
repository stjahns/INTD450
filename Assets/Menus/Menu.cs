using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	public int play;
	public string anim;
	public Sprite click;
	public Sprite original;
	public AudioClip onclick;
	///public Sprite hover;

	void  OnMouseDown() {
		Debug.Log (gameObject.name);
		SpriteRenderer render = gameObject.GetComponent <SpriteRenderer>();
		AudioSource.PlayClipAtPoint (onclick, transform.position);
		double b = -20;
		GameObject Camera;
		Vector3 v = new Vector3((float)b,0,0);
		transform.Translate(v * 1 * Time.deltaTime); 


		///animation["MainAnim"].speed = 0.5;
	}
	
	void  OnMouseUp() {
		SpriteRenderer render = gameObject.GetComponent <SpriteRenderer>();
		render.sprite = original;
		GameObject Camera;
		double b = 20;
		Vector3 v = new Vector3((float)b,0,0);
		transform.Translate(v * 1 * Time.deltaTime);
		if (gameObject.name == "SelectLevel" || gameObject.name == "Back") {
						Camera = GameObject.FindGameObjectWithTag ("Cam");
						if (play == 1 && anim == "CamerAnim") {
								Camera.animation.Play ("CamerAnim");
						} else {
								if (play == 1 && anim == "CamerAnim2") {
										Camera.animation.Play ("CamerAnim2");
								}
						}
				} else 
				{
					if (gameObject.name == "start")
					{
						Save_Load load = new Save_Load();
						load.player_name="player";
						load.create_new();
						Application.LoadLevel(1);
					}
					if (gameObject.name == "Exit")
						{

						}

					if (gameObject.name == "resume")
					{
						Save_Load load = new Save_Load();
						load.player_name="player";
						var data=load.file_load();
						Debug.Log(data);
						int level=System.Convert.ToInt32(data["array"][1]["Level"]);
						if (level==0){
							level+=2;
						}
						Application.LoadLevel(level);
					}

					
					}
		
	}
}
