
using UnityEngine;
using System.Collections;

public class moving_plates : MonoBehaviour {
	
	public GameObject moving_plate;
	public float bouncespeed = 0.002f;
	private bool itemBounceUp =true;
	public float Max_y;
	private float temp_max;
	private float temp_y;
	void Start () {
		StartCoroutine(itembounce());
		Vector3 temp = moving_plate.transform.position;
		temp_y = temp.y;
		temp_max = Max_y;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 temp = moving_plate.transform.position;
		///Debug.Log (temp.y);
		if (temp.y <= Max_y && itemBounceUp==true ) 
		{
			Max_y=temp_max;
			temp.y += bouncespeed;
		}
		if (temp.y > Max_y && !itemBounceUp==false)
		{
			
			Max_y=temp_y;
			temp.y-= bouncespeed;
		}
		
		moving_plate.transform.position = temp;
	}
	IEnumerator  itembounce () {
		
		yield return new WaitForSeconds (0.2f);
		itemBounceUp = false;
		yield return new WaitForSeconds (0.2f);
		itemBounceUp = true;
		
	}
	
}
