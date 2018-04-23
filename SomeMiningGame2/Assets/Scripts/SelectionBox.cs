using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionBox : Tile {

	public enum SelectionBoxType {
		unset,
		add_new_job_marker,
		hover_job_marker,
		idle_job_marker
	}

	public SelectionBoxType selection_box_type;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
