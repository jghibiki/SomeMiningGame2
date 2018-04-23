using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelManager : MonoBehaviour {

	private List<string> alert_messages = new List<string>();
	private List<int> alert_timers = new List<int>();
	private int alert_display_time = 100;
	private int max_alert_messages = 5;
	private bool update_alerts = false;
	public Text alert_message_text;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		List<int> messages_to_remove = new List<int>();

		for(int i=0; i<alert_messages.Count; i++){
			alert_timers[i] -= 1;
			if(alert_timers[i] <= 0){
				messages_to_remove.Add(i);
			}
		}

		for(int i=messages_to_remove.Count-1; i>=0; i--){
			Debug.Log(messages_to_remove[i]);
			alert_messages.RemoveAt(messages_to_remove[i]);
			alert_timers.RemoveAt(messages_to_remove[i]);

			update_alerts = true;
		}
		

		if(update_alerts){
			var output = "";

			foreach(var msg in alert_messages){
				output += "- " + msg + "\n";
			}

			alert_message_text.text = output;
			update_alerts = false;
		}
	}

	public void AddAlert(string msg){
		if(alert_messages.Count > max_alert_messages){
			alert_messages.RemoveAt(0);
			alert_timers.RemoveAt(0);
		}

		alert_messages.Add(msg);
		alert_timers.Add(alert_display_time);

		update_alerts = true;
	}
}
