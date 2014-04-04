using UnityEngine;
using System.Collections;


public class TriggerSave : MonoBehaviour {

	public MeshRenderer savedTextRenderer;
	public TextMesh savedTextMesh;
	public float savedTextShowTime;

	void Start()
	{
		savedTextRenderer.enabled = false;
		savedTextRenderer.sortingLayerName = "UI";
	}

	// Use this for initialization
	void OnTriggerEnter2D (Collider2D other){
		string checkpoint = "";
		string boxes_checkpoint = "";

		if (other.gameObject.GetComponent<RobotComponent>() != null && other.gameObject.GetComponent<RobotComponent>().attachedToPlayer())
		{
			PlayerBehavior player = PlayerBehavior.Player;
			RobotComponent[] obj = FindObjectsOfType(typeof(RobotComponent)) as RobotComponent[];
			BoxComponent[] level_boxes = FindObjectsOfType(typeof(BoxComponent)) as BoxComponent[];        
			// Show "Checkpoint Saved" text above checkpoint briefly
			StartCoroutine(ShowSavedText());

			int level = Application.loadedLevel;
			Save_Load save = new Save_Load ();
			save.player_name="player";

			foreach (RobotComponent gam in obj)
			{
				checkpoint += gam.name + ":" + gam.transform.rotation.ToString() + ":" + gam.transform.position.ToString() + "/";
			}

			if (level_boxes.Length >=1)
			{
				foreach (BoxComponent box in level_boxes)
				{
					boxes_checkpoint += "Box:" + box.transform.rotation.ToString() + ":" + box.transform.position.ToString() + "/";
				}
			}

			if (player != null)
			{
				save.add_checkpoint(level, checkpoint, boxes_checkpoint, player.transform.position);
			}
		}
	}

	void OnTriggerExit2D (Collider2D other){

		Save_Load save = new Save_Load ();
		save.player_name="player";

	}

	IEnumerator ShowSavedText()
	{
		savedTextRenderer.enabled = true;

		Color color = new Color(1, 1, 1, 0);
		savedTextMesh.color = color;

		// fade in
		float startTime = Time.time;
		while (Time.time < startTime + savedTextShowTime / 2)
		{
			color.a = Mathf.Lerp(0, 1, (Time.time - startTime) / (savedTextShowTime / 2));
			savedTextMesh.color = color;
			yield return 0;
		}

		yield return new WaitForSeconds(savedTextShowTime);

		// fade out
		startTime = Time.time;
		while (Time.time < startTime + savedTextShowTime)
		{
			color.a = Mathf.Lerp(1, 0, (Time.time - startTime) / (savedTextShowTime / 2));
			savedTextMesh.color = color;
			yield return 0;
		}

		savedTextRenderer.enabled = false;
	}
}
