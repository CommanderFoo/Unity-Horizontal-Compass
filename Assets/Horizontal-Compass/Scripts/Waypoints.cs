using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using net.pixeldepth.compass;
using TMPro;

// Example of how to add waypoints to the compass.

public class Waypoints : MonoBehaviour {
	
	public Transform icon;
	public Transform obj;

	public Transform icon2;
	public Transform obj2;

	void Start(){
		Horizontal_Compass.instance.add("test", new Waypoint(){
			
			icon = this.icon,
			target = this.obj

		});

		Horizontal_Compass.instance.add("test2", new Waypoint(){

			icon = this.icon2,
			target = this.obj2

		});
	}
	
}
