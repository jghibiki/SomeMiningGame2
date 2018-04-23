using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour {

	public bool paused = false;

	public Text pause_text;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown("space")){
			paused = !paused;

		}
		
		if(paused){
			pause_text.color = new Color(0f/255f, 255f/255f, 0/255f, 255f/255f);
		}
		else{
			pause_text.color = new Color(0f/255f, 101f/255f, 0/255f, 255f/255f);
		}
	}
}
