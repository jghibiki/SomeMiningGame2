using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerActionLine : MonoBehaviour {

	LineRenderer line_renderer;	

	Vector2 pos1;
	Vector2 pos2;

	// Use this for initialization
	void Start () {
		line_renderer = GetComponent<LineRenderer>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetPoints(Vector2 pos1, Vector2 pos2){
		this.pos1 = pos1;
		this.pos2 = pos2;

		if(line_renderer == null){
			line_renderer = GetComponent<LineRenderer>();
		}

		line_renderer.SetPosition(0, new Vector3(pos1.x, pos1.y, -6));
		line_renderer.SetPosition(1, new Vector3(pos2.x, pos2.y, -6));
	}
}
