using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanCamera : MonoBehaviour {

	public float pan_speed = 1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKey("w")){
			transform.position += Vector3.up * pan_speed * Time.deltaTime;
		}
		
		if(Input.GetKey("s")){
			transform.position += -Vector3.up * pan_speed * Time.deltaTime;
		}
		
		if(Input.GetKey("a")){
			transform.position += Vector3.left * pan_speed * Time.deltaTime;
		}

		if(Input.GetKey("d")){
			transform.position += -Vector3.left * pan_speed * Time.deltaTime;
		}
	}
}
