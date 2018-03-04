using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using DG.Tweening;

// TODO:  Comment methods (sorry, I am lazy)

namespace net.pixeldepth.compass {
	
	public class Waypoint {

		public Transform icon;
		public Transform target;
		public Transform obj;
		public CanvasGroup grp;

	}

	public class Horizontal_Compass : MonoBehaviour {
	
		public static Horizontal_Compass instance;

		public Transform player;

		[Space]
		[Header("Poles")]

		// Poles UI icon

		public Transform north;
		public Transform south;
		public Transform east;
		public Transform west;
				
		[Space]
		[Header("Waypoints")]

		public Transform container;

		[Space]
		
		public Dictionary<string, Waypoint> waypoints = new Dictionary<string, Waypoint>();
		public Dictionary<string, Waypoint> poles = new Dictionary<string, Waypoint>();

		private float width = 0f;
		private float width_offset = 15f;

		private void Awake(){
			if(instance != null && instance != this){
				Debug.LogWarning("More than one Compass instance?");
				return;
			}

			instance = this;
		}

		private void Start(){
			this.width = this.GetComponent<RectTransform>().sizeDelta.x - this.width_offset;

			this.poles.Add("north", new Waypoint(){

				icon = this.north,
				target = this.player,
				obj = this.north,
				grp = this.north.GetComponent<CanvasGroup>()

			});

			this.poles.Add("south", new Waypoint(){

				icon = this.south,
				target = this.player,
				obj = this.south,
				grp = this.south.GetComponent<CanvasGroup>()

			});

			this.poles.Add("east", new Waypoint(){

				icon = this.east,
				target = this.player,
				obj = this.east,
				grp = this.east.GetComponent<CanvasGroup>()

			});

			this.poles.Add("west", new Waypoint(){

				icon = this.west,
				target = this.player,
				obj = this.west,
				grp = this.west.GetComponent<CanvasGroup>()

			});
			
			this.set_poles();
		}

		public void Update(){
			this.set_poles();

			if(this.waypoints.Count > 0){
				List<string> waypoints_keys = new List<string>(this.waypoints.Keys);

				foreach(string key in waypoints_keys){
					this.set_position(key, Vector3.zero);
				}
			}
		}

		private void set_poles(){
			this.set_position("north", Vector3.forward, true);
			this.set_position("south", Vector3.back, true);
			this.set_position("east", Vector3.right, true);
			this.set_position("west", Vector3.left, true);
		}
		
		private void set_position(string key, Vector3 additional, bool pole = false){
			Waypoint waypoint = null;

			if(pole){
				if(!this.poles.TryGetValue(key, out waypoint)){
					return;
				}
			} else if(!this.waypoints.TryGetValue(key, out waypoint)){
				return;
			}

			if(waypoint == null || waypoint.target == null){
				return;
			}			
			
			Vector3 offset = player.InverseTransformPoint(waypoint.target.position + additional);
			float angle = Mathf.Atan2(offset.x, offset.z);
			Vector3 pos = Vector3.right * (this.width * 2) * angle / (2f * Mathf.PI);

			if(waypoint.obj != null){
				waypoint.obj.localPosition = new Vector3(pos.x, waypoint.obj.localPosition.y, 0);
			}

			if(waypoint.grp != null){
				if(angle < -1.58f || angle > 1.58f){
					waypoint.grp.DOFade(0f, 0.3f);
				} else if(waypoint.grp.alpha < 1f){
					waypoint.grp.DOFade(1f, 0.6f);
				}
			}

			if(pole){
				this.poles[key] = waypoint;				
			} else {
				this.waypoints[key] = waypoint;
			}
		}

		public bool add(string key, Waypoint waypoint, bool fade_in = true){
			if(this.waypoints.ContainsKey(key)){
				return false;
			}

			waypoint.obj = Instantiate(waypoint.icon, this.container, false);
			waypoint.grp = waypoint.obj.GetComponent<CanvasGroup>();

			waypoint.grp.alpha = 0f;

			this.waypoints.Add(key, waypoint);			

			if(fade_in){
				Sequence seq = DOTween.Sequence().Pause();

				seq.Append(waypoint.grp.DOFade(0f, .4f));
				seq.Append(waypoint.grp.DOFade(1f, .4f)).SetLoops(4);

				waypoint.grp.DOFade(1f, 1f).SetDelay(1f).OnComplete(() => seq.Play());
			} else {
				waypoint.grp.alpha = 1f;
			}

			return true;
		}

		public Waypoint get(string key){
			Waypoint waypoint = null;

			this.waypoints.TryGetValue(key, out waypoint);

			return waypoint;
		}

		public bool set_color(string key, Color color){
			Waypoint waypoint = null;

			if(this.waypoints.TryGetValue(key, out waypoint)){
				waypoint.obj.GetComponent<TextMeshProUGUI>().color = color;
			}

			return false;
		}

		public void clear(){
			this.waypoints.Clear();

			foreach(Transform child in this.container){
				Destroy(child);
			}			
		}

		public bool remove(string key){
			Waypoint waypoint = null;

			this.waypoints.TryGetValue(key, out waypoint);

			if(waypoint != null){
				this.waypoints.Remove(key);

				waypoint.grp.DOFade(0f, 1f).OnComplete(() => {
					Destroy(waypoint.obj);
				});
	
				return true;
			}

			return false;
		}

	}

}