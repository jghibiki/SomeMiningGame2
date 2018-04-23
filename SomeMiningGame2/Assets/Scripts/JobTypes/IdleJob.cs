using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleJob : Job {

	private int idle_turns = (int)Random.Range(3f, 5f);
	private Worker unit;

	public IdleJob(Worker unit, string description) 
		: base (new Target((int)unit.transform.position.x, (int)unit.transform.position.y, (int)unit.transform.position.z),
				description,
				-99,
				false ){

		this.unit = unit;
	}

	public override GenericResult Do(){
		idle_turns -= 1;
		if(idle_turns <= 0){
			done = true;
			EndJob();
		}

		return new NoResult();
	}

}
