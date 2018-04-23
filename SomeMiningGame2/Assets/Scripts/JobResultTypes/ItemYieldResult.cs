using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemYieldResult : GenericResult {

	public Item item_type;
	public int yield;

	public ItemYieldResult(Item item_type, int yield){
		this.item_type = item_type;
		this.yield = yield;
	}
}
