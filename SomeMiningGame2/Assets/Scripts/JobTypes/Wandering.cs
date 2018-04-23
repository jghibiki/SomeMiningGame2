using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wandering : IdleJob {


	public Wandering(Worker unit): base(unit, "Wandering around."){

		GameObject map_gen_game_object = GameObject.Find("MapGenerator");
		MapGenerator map_generator = map_gen_game_object.GetComponent<MapGenerator>();

		float[] signs = new float[] {-1, 1};

		float x_sign = signs[(int)Random.Range(0f,2f)];
		float y_sign = signs[(int)Random.Range(0f,2f)];

		float offset_x = Random.Range(2f, 5f) * x_sign;
		float offset_y = Random.Range(2f, 5f) * y_sign;

		float start_pos_center_x = map_generator.start_position_bottom_right.x - map_generator.start_position_top_left.x;
		float start_pos_center_y = map_generator.start_position_bottom_right.y - map_generator.start_position_top_left.y;

		int new_x = (int)(map_generator.start_position_top_left.x + start_pos_center_x + offset_x);
		int new_y = (int)(map_generator.start_position_top_left.y + start_pos_center_y + offset_y);

		Target new_target = new Target(
			new_x,
			new_y,
			(int)unit.transform.position.z
		);

		UpdateTarget(new_target);
	}

	public override GenericResult Do(){
		done = true;
		EndJob();
		return new NoResult();
	}

	

}
