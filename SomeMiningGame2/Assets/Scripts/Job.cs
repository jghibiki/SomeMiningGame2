using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public abstract class Job {

	public bool done = false;
	public bool aborted = false;
	public bool	show_work_animation = true;
	private Target target;
	private string description;
	public int priority = 0;
	private Worker assignee;


	public Job(Target target, string description){
		this.target = target;
		this.description = description;
	}

	public Job(Target target, string description, int priority){
		this.target = target;
		this.description = description;
		this.priority = priority;
	}

	public Job(Target target, string description, int priority, bool show_work_animation){
		this.target = target;
		this.description = description;
		this.priority = priority;
		this.show_work_animation = show_work_animation;
	}

	public void AssignWorker(Worker assignee){
		this.assignee = assignee;
		assignee.AssignJob(this);
	}

	public abstract GenericResult Do();


	public void Abort(string reason=null){
		this.aborted = true;
		EndJob();
	}

	public void Cancel(){
		this.done = true;
		EndJob();
	}

	public void Postpone(string reason=null){
		this.priority = -2;
	}

	protected void EndJob(){
		if(assignee != null){
			assignee.ClearJob();
			assignee = null;
		}
	}

	public void UpdateTarget(Target target){
		this.target = target;
	}

	public Target GetTarget(){
		return target;
	}

}
