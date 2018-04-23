using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherItemJob : Job {

	Item item_type;
	int yield;

	int work_required = 10;
	int work_completed = 0;

	public GatherItemJob(Target target, Item item_type, int yield) : base (target, "Gathering " + item_type.name + ".", 2){

		this.item_type = item_type;
		this.yield = yield;

	}

	public override GenericResult Do(){

		work_completed += 1;
		if(work_completed >= work_required){
			done = true;
			EndJob();
		}

		return new ItemYieldResult(item_type, yield/work_required);
	}

}
