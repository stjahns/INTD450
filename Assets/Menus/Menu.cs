using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	void  OnMouseEnter() {
		double b = -20;
		Vector3 v = new Vector3((float)b,0,0);
		transform.Translate(v * 1 * Time.deltaTime); 
		renderer.material.color= Color.blue;
	}
	
	void  OnMouseExit() {
		double b = 20;
		Vector3 v = new Vector3((float)b,0,0);
		transform.Translate(v * 1 * Time.deltaTime);
		renderer.material.color= Color.white;
		
	}
}
